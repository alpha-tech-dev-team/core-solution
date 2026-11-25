using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.TgBot.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Pandora.Domain.Entities;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;
using Pandora.Domain.Options;
using Pandora.Infrastructure.Database;
using Pandora.TgBot.Application.Features.Credentials;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Xunit;

namespace CoreSolution.Integration.Tests.PandoraTests;

[Collection(nameof(IntegrationCollection))]
public sealed class CredentialMessageCleanupTests(IntegrationFixture fixture)
{
    private const long ChatId = 9101;

    [Fact]
    public async Task ProcessDue_DeletesCredentialsMessageAndSendsExpiryNotice()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();

        await ResetMessagesAsync(provider);

        var messageId = 501;
        var messageIdStored = await AddMessageAsync(provider, messageId);
        var gateway = new FakeCredentialMessageGateway { DeleteOutcome = CredentialMessageOutcome.Succeeded };
        var notifications = new FakeNotifier();

        await ProcessDueAsync(provider, gateway, notifications);

        Assert.Single(gateway.DeleteCalls);
        Assert.Equal(messageId, gateway.DeleteCalls[0].MessageId);
        Assert.Equal(ChatId, gateway.DeleteCalls[0].ChatId);
        Assert.Single(notifications.SentMessages);
        Assert.Contains("TestService", notifications.SentMessages[0]);

        var stored = await GetMessageAsync(provider, messageIdStored);

        Assert.NotNull(stored);
        Assert.NotNull(stored.DeletedAt);
        Assert.NotNull(stored.NotifiedAt);
    }

    [Fact]
    public async Task ProcessDue_WhenDeleteFails_PostponesRetryAndKeepsMessageScheduled()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();

        await ResetMessagesAsync(provider);

        var messageIdStored = await AddMessageAsync(provider, 502);
        var gateway = new FakeCredentialMessageGateway { DeleteOutcome = CredentialMessageOutcome.Failed };
        var notifications = new FakeNotifier();

        await ProcessDueAsync(provider, gateway, notifications);

        Assert.Single(gateway.DeleteCalls);
        Assert.Empty(gateway.ReplaceCalls);
        Assert.Empty(notifications.SentMessages);

        var stored = await GetMessageAsync(provider, messageIdStored);

        Assert.NotNull(stored);
        Assert.Null(stored.DeletedAt);
        Assert.Equal(1, stored.Attempts);
        Assert.NotNull(stored.LastError);
        Assert.True(stored.NextAttemptAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task ProcessDue_WhenMessageIsMissing_MarksRowHandledWithoutNotice()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();

        await ResetMessagesAsync(provider);

        var messageIdStored = await AddMessageAsync(provider, 503);
        var gateway = new FakeCredentialMessageGateway { DeleteOutcome = CredentialMessageOutcome.NotFound };
        var notifications = new FakeNotifier();

        await ProcessDueAsync(provider, gateway, notifications);

        Assert.Single(gateway.DeleteCalls);
        Assert.Empty(notifications.SentMessages);

        var stored = await GetMessageAsync(provider, messageIdStored);

        Assert.NotNull(stored);
        Assert.NotNull(stored.DeletedAt);
        Assert.NotNull(stored.NotifiedAt);
    }

    [Fact]
    public async Task ProcessDue_WhenDeleteKeepsFailing_ReplacesCredentialsWithExpiryNotice()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();

        await ResetMessagesAsync(provider);

        var messageIdStored = await AddMessageAsync(provider, 504, attempts: 2);
        var gateway = new FakeCredentialMessageGateway
        {
            DeleteOutcome = CredentialMessageOutcome.Failed,
            ReplaceOutcome = CredentialMessageOutcome.Succeeded
        };
        var notifications = new FakeNotifier();

        await ProcessDueAsync(provider, gateway, notifications);

        Assert.Single(gateway.DeleteCalls);
        Assert.Single(gateway.ReplaceCalls);
        Assert.Contains("TestService", gateway.ReplaceCalls[0].Text);
        Assert.Empty(notifications.SentMessages);

        var stored = await GetMessageAsync(provider, messageIdStored);

        Assert.NotNull(stored);
        Assert.NotNull(stored.DeletedAt);
        Assert.NotNull(stored.NotifiedAt);
    }

    [Fact]
    public async Task ScheduleAsync_StoresRowWithConfiguredTtl()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();

        await ResetMessagesAsync(provider);
        await UpdateTtlAsync(provider, 5);

        var gateway = new FakeCredentialMessageGateway();

        using (var scope = provider.CreateScope())
        {
            var scheduler = new CredentialMessageScheduler(
                scope.ServiceProvider.GetRequiredService<ICredentialAccessSettingsRepository>(),
                scope.ServiceProvider.GetRequiredService<ICredentialAccessMessageRepository>(),
                scope.ServiceProvider.GetRequiredService<ISecretMetadataRepository>(),
                gateway,
                CreateNoticeBuilder(scope.ServiceProvider),
                NullLogger<CredentialMessageScheduler>.Instance);

            var scheduled = await scheduler.ScheduleAsync(ChatId, 601, "TestService", CancellationToken.None);

            Assert.True(scheduled);
        }

        using (var scope = provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();

            var stored = await context.CredentialAccessMessage
                .AsNoTracking()
                .SingleAsync(message => message.MessageId == 601);

            Assert.Equal(5, (stored.ExpiresAt - stored.RequestedAt).TotalMinutes, 1);
            Assert.Equal(stored.ExpiresAt, stored.NextAttemptAt);
            Assert.False(string.IsNullOrWhiteSpace(stored.ItemName));
        }
    }

    [Fact]
    public async Task ScheduleAsync_WhenPersistFails_DeletesCredentialsMessageImmediately()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();

        await ResetMessagesAsync(provider);

        var gateway = new FakeCredentialMessageGateway();

        using (var scope = provider.CreateScope())
        {
            var scheduler = new CredentialMessageScheduler(
                scope.ServiceProvider.GetRequiredService<ICredentialAccessSettingsRepository>(),
                new FailingMessageRepository(),
                scope.ServiceProvider.GetRequiredService<ISecretMetadataRepository>(),
                gateway,
                CreateNoticeBuilder(scope.ServiceProvider),
                NullLogger<CredentialMessageScheduler>.Instance);

            var scheduled = await scheduler.ScheduleAsync(ChatId, 602, "TestService", CancellationToken.None);

            Assert.False(scheduled);
        }

        Assert.Single(gateway.DeleteCalls);
        Assert.Equal(602, gateway.DeleteCalls[0].MessageId);
        Assert.Empty(gateway.ReplaceCalls);
    }

    [Fact]
    public async Task ScheduleAsync_WhenPersistAndDeleteFail_ReplacesCredentialsWithNotice()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();

        await ResetMessagesAsync(provider);

        var gateway = new FakeCredentialMessageGateway
        {
            DeleteOutcome = CredentialMessageOutcome.Failed
        };

        using (var scope = provider.CreateScope())
        {
            var scheduler = new CredentialMessageScheduler(
                scope.ServiceProvider.GetRequiredService<ICredentialAccessSettingsRepository>(),
                new FailingMessageRepository(),
                scope.ServiceProvider.GetRequiredService<ISecretMetadataRepository>(),
                gateway,
                CreateNoticeBuilder(scope.ServiceProvider),
                NullLogger<CredentialMessageScheduler>.Instance);

            var scheduled = await scheduler.ScheduleAsync(ChatId, 603, "TestService", CancellationToken.None);

            Assert.False(scheduled);
        }

        Assert.Single(gateway.DeleteCalls);
        Assert.Single(gateway.ReplaceCalls);
        Assert.Contains("TestService", gateway.ReplaceCalls[0].Text);
    }

    [Fact]
    public async Task ProcessDue_WhenAttemptsExhausted_SkipsMessage()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();

        await ResetMessagesAsync(provider);

        var maxAttempts = new CredentialCleanupOptions().MaxAttempts;
        var messageIdStored = await AddMessageAsync(provider, 505, attempts: maxAttempts);
        var gateway = new FakeCredentialMessageGateway { DeleteOutcome = CredentialMessageOutcome.Failed };
        var notifications = new FakeNotifier();

        await ProcessDueAsync(provider, gateway, notifications);

        Assert.Empty(gateway.DeleteCalls);
        Assert.Empty(notifications.SentMessages);

        var stored = await GetMessageAsync(provider, messageIdStored);

        Assert.NotNull(stored);
        Assert.Equal(maxAttempts, stored.Attempts);
    }

    [Fact]
    public async Task ProcessDue_RemovesExhaustedRowsOlderThanRetention()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();

        await ResetMessagesAsync(provider);

        var options = new CredentialCleanupOptions();
        var oldAttemptAt = DateTime.UtcNow.AddHours(-(options.CompletedRetentionHours + 1));
        var oldId = await AddMessageAsync(
            provider,
            506,
            attempts: options.MaxAttempts,
            nextAttemptAt: oldAttemptAt);
        var gateway = new FakeCredentialMessageGateway();
        var notifications = new FakeNotifier();

        await ProcessDueAsync(provider, gateway, notifications);

        Assert.Null(await GetMessageAsync(provider, oldId));
    }

    [Fact]
    public async Task ProcessDue_RemovesCompletedRowsOlderThanRetention()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();

        await ResetMessagesAsync(provider);

        var oldId = await AddCompletedMessageAsync(provider, 701, DateTime.UtcNow.AddHours(-2));
        var freshId = await AddCompletedMessageAsync(provider, 702, DateTime.UtcNow);

        using (var scope = provider.CreateScope())
        {
            var service = new CredentialMessageCleanupService(
                scope.ServiceProvider.GetRequiredService<ICredentialAccessMessageRepository>(),
                new FakeCredentialMessageGateway(),
                new FakeNotifier(),
                CreateNoticeBuilder(scope.ServiceProvider),
                Options.Create(new CredentialCleanupOptions { CompletedRetentionHours = 1 }),
                NullLogger<CredentialMessageCleanupService>.Instance);

            await service.ProcessDueAsync(DateTime.UtcNow, CancellationToken.None);
        }

        Assert.Null(await GetMessageAsync(provider, oldId));
        Assert.NotNull(await GetMessageAsync(provider, freshId));
    }

    [Fact]
    public async Task ProcessDue_WhenRowAlreadyClaimedByAnotherWorker_SkipsIt()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();

        await ResetMessagesAsync(provider);

        var messageIdStored = await AddMessageAsync(provider, 507, claimedUntil: DateTime.UtcNow.AddMinutes(1));
        var gateway = new FakeCredentialMessageGateway();
        var notifications = new FakeNotifier();

        await ProcessDueAsync(provider, gateway, notifications);

        Assert.Empty(gateway.DeleteCalls);
        Assert.Empty(notifications.SentMessages);

        var stored = await GetMessageAsync(provider, messageIdStored);

        Assert.NotNull(stored);
        Assert.Null(stored.DeletedAt);
    }

    [Fact]
    public async Task ClaimDue_WhenTwoWorkersRace_OnlyOneClaimsRow()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();

        await ResetMessagesAsync(provider);

        await AddMessageAsync(provider, 508);

        var now = DateTime.UtcNow;
        var claimUntil = now.AddMinutes(2);

        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();
        var firstRepository = firstScope.ServiceProvider.GetRequiredService<ICredentialAccessMessageRepository>();
        var secondRepository = secondScope.ServiceProvider.GetRequiredService<ICredentialAccessMessageRepository>();

        var claims = await Task.WhenAll(
            ClaimAsync(firstRepository),
            ClaimAsync(secondRepository));

        Assert.Equal(1, claims.Sum(batch => batch.Count));

        Task<IReadOnlyList<CredentialAccessMessage>> ClaimAsync(ICredentialAccessMessageRepository repository)
        {
            return repository.ClaimDue(
                new CredentialClaim(now, claimUntil, 10, 10),
                CancellationToken.None);
        }
    }

    [Fact]
    public async Task MarkFailed_WhenAttemptsChangedConcurrently_DoesNotOverwriteNewerState()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();

        await ResetMessagesAsync(provider);

        var messageIdStored = await AddMessageAsync(provider, 509);
        var nextAttemptAt = DateTime.UtcNow.AddMinutes(1);

        using (var scope = provider.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<ICredentialAccessMessageRepository>();

            Assert.True(await repository.MarkFailed(
                messageIdStored,
                new CredentialFailure(0, "first failure", nextAttemptAt),
                CancellationToken.None));
            Assert.False(await repository.MarkFailed(
                messageIdStored,
                new CredentialFailure(0, "stale failure", nextAttemptAt),
                CancellationToken.None));
        }

        var stored = await GetMessageAsync(provider, messageIdStored);

        Assert.NotNull(stored);
        Assert.Equal(1, stored.Attempts);
        Assert.Equal("first failure", stored.LastError);
    }

    [Fact]
    public async Task ProcessDue_WhenCancellationRequested_PropagatesCancellation()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();

        await ResetMessagesAsync(provider);

        await AddMessageAsync(provider, 510);

        using var scope = provider.CreateScope();
        using var cancellation = new CancellationTokenSource();
        var gateway = new FakeCredentialMessageGateway();
        var notifier = new CancellingNotifier(cancellation);

        var service = new CredentialMessageCleanupService(
            scope.ServiceProvider.GetRequiredService<ICredentialAccessMessageRepository>(),
            gateway,
            notifier,
            CreateNoticeBuilder(scope.ServiceProvider),
            Options.Create(new CredentialCleanupOptions()),
            NullLogger<CredentialMessageCleanupService>.Instance);

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => service.ProcessDueAsync(DateTime.UtcNow, cancellation.Token));
    }

    [Fact]
    public async Task ClaimDue_WhenClaimExpires_TakesRowAgain()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();

        await ResetMessagesAsync(provider);

        var messageIdStored = await AddMessageAsync(provider, 511, claimedUntil: DateTime.UtcNow.AddMinutes(-1));
        var gateway = new FakeCredentialMessageGateway { DeleteOutcome = CredentialMessageOutcome.Succeeded };
        var notifications = new FakeNotifier();

        await ProcessDueAsync(provider, gateway, notifications);

        Assert.Single(gateway.DeleteCalls);

        var stored = await GetMessageAsync(provider, messageIdStored);

        Assert.NotNull(stored);
        Assert.NotNull(stored.DeletedAt);
    }

    private static CredentialAccessNoticeBuilder CreateNoticeBuilder(IServiceProvider services)
    {
        return new CredentialAccessNoticeBuilder(services.GetRequiredService<ILocalizer>());
    }

    private static async Task ProcessDueAsync(
        IServiceProvider provider,
        FakeCredentialMessageGateway gateway,
        FakeNotifier notifications)
    {
        using var scope = provider.CreateScope();

        var service = new CredentialMessageCleanupService(
            scope.ServiceProvider.GetRequiredService<ICredentialAccessMessageRepository>(),
            gateway,
            notifications,
            CreateNoticeBuilder(scope.ServiceProvider),
            Options.Create(new CredentialCleanupOptions()),
            NullLogger<CredentialMessageCleanupService>.Instance);

        await service.ProcessDueAsync(DateTime.UtcNow, CancellationToken.None);
    }

    private static async Task<Guid> AddMessageAsync(
        IServiceProvider provider,
        int messageId,
        int attempts = 0,
        DateTime? nextAttemptAt = null,
        DateTime? claimedUntil = null)
    {
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var message = new CredentialAccessMessage(
            ChatId,
            messageId,
            "TestService",
            "TestService",
            now.AddMinutes(-5),
            now.AddMinutes(-1))
        {
            Id = id,
            Attempts = attempts,
            NextAttemptAt = nextAttemptAt ?? now.AddMinutes(-1),
            ClaimedUntil = claimedUntil
        };

        using var scope = provider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICredentialAccessMessageRepository>();

        await repository.Add(message, CancellationToken.None);

        return id;
    }

    private static async Task<Guid> AddCompletedMessageAsync(
        IServiceProvider provider,
        int messageId,
        DateTime completedAt)
    {
        var id = Guid.NewGuid();
        var message = new CredentialAccessMessage(
            ChatId,
            messageId,
            "TestService",
            "TestService",
            completedAt.AddMinutes(-5),
            completedAt.AddMinutes(-1))
        {
            Id = id,
            NextAttemptAt = completedAt,
            DeletedAt = completedAt,
            NotifiedAt = completedAt
        };

        using var scope = provider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICredentialAccessMessageRepository>();

        await repository.Add(message, CancellationToken.None);

        return id;
    }

    private static async Task<CredentialAccessMessage?> GetMessageAsync(IServiceProvider provider, Guid id)
    {
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();

        return await context.CredentialAccessMessage
            .AsNoTracking()
            .FirstOrDefaultAsync(message => message.Id == id);
    }

    private static async Task ResetMessagesAsync(IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();

        context.CredentialAccessMessage.RemoveRange(context.CredentialAccessMessage);

        await context.SaveChangesAsync();
    }

    private static async Task UpdateTtlAsync(IServiceProvider provider, int ttlMinutes)
    {
        using var scope = provider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICredentialAccessSettingsRepository>();

        await repository.Update(ttlMinutes, null, CancellationToken.None);
    }

    private sealed class FailingMessageRepository : ICredentialAccessMessageRepository
    {
        public Task Add(CredentialAccessMessage message, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Хранилище недоступно");
        }

        public Task<IReadOnlyList<CredentialAccessMessage>> ClaimDue(
            CredentialClaim claim,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<CredentialAccessMessage>>(Array.Empty<CredentialAccessMessage>());
        }

        public Task<int> DeleteCompleted(DateTime before, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public Task<int> DeleteExhausted(int maxAttempts, DateTime before, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public Task MarkDeleted(
            Guid id,
            DateTime deletedAt,
            DateTime? notifiedAt,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task MarkNotified(Guid id, DateTime notifiedAt, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<bool> MarkFailed(
            Guid id,
            CredentialFailure failure,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }

    private sealed class FakeCredentialMessageGateway : ICredentialMessageGateway
    {
        public CredentialMessageOutcome DeleteOutcome { get; set; } = CredentialMessageOutcome.Succeeded;
        public CredentialMessageOutcome ReplaceOutcome { get; set; } = CredentialMessageOutcome.Succeeded;
        public List<(long ChatId, int MessageId)> DeleteCalls { get; } = new();
        public List<(long ChatId, int MessageId, string Text)> ReplaceCalls { get; } = new();

        public Task<CredentialMessageOutcome> Delete(
            long chatId,
            int messageId,
            CancellationToken cancellationToken)
        {
            DeleteCalls.Add((chatId, messageId));

            return Task.FromResult(DeleteOutcome);
        }

        public Task<CredentialMessageOutcome> ReplaceText(
            long chatId,
            int messageId,
            string text,
            CancellationToken cancellationToken)
        {
            ReplaceCalls.Add((chatId, messageId, text));

            return Task.FromResult(ReplaceOutcome);
        }
    }

    private sealed class FakeNotifier : ICredentialMessageNotifier
    {
        public List<string> SentMessages { get; } = new();

        public Task SendNotice(long chatId, string text, CancellationToken cancellationToken)
        {
            SentMessages.Add(text);

            return Task.CompletedTask;
        }
    }

    private sealed class CancellingNotifier(CancellationTokenSource cancellation) : ICredentialMessageNotifier
    {
        public Task SendNotice(long chatId, string text, CancellationToken cancellationToken)
        {
            cancellation.Cancel();

            throw new OperationCanceledException(cancellationToken);
        }
    }
}
