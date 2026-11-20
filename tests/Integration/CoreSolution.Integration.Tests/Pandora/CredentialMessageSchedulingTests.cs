using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using CoreSolution.Shared.TgBot.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;
using Pandora.Infrastructure.Database;
using Pandora.TgBot.Application.Common;
using Pandora.TgBot.BotHandlers;
using Pandora.TgBot.Credentials;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Xunit;

namespace CoreSolution.Integration.Tests.PandoraTests;

[Collection(nameof(IntegrationCollection))]
public sealed class CredentialMessageSchedulingTests(IntegrationFixture fixture)
{
    private const long ChatId = 9201;
    private const int SentMessageId = 8123;
    private const string ItemId = "TestService";

    [Fact]
    public async Task HandleAsync_WhenCredentialsSent_SchedulesCleanupWithConfiguredTtl()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();

        await ResetMessagesAsync(provider);
        await UpdateTtlAsync(provider, 2);

        var notifications = new StubNotificationService(SentMessageId);

        using var scope = provider.CreateScope();

        var handler = new ServiceCallbackBotHandler(
            notifications,
            new StubMediator(CreateCredentialsResponse()),
            scope.ServiceProvider.GetRequiredService<IUserErrorMapper>(),
            CreateScheduler(scope));

        await handler.HandleAsync(CreateCallbackUpdate(ChatId), CancellationToken.None);

        Assert.Single(notifications.SentMessages);

        using var verifyScope = provider.CreateScope();
        var context = verifyScope.ServiceProvider.GetRequiredService<PandoraDbContext>();

        var stored = await context.CredentialAccessMessage
            .AsNoTracking()
            .SingleAsync(message => message.MessageId == SentMessageId);

        Assert.Equal(ChatId, stored.ChatId);
        Assert.Equal(ItemId, stored.ItemId);
        Assert.Equal(2, (stored.ExpiresAt - stored.RequestedAt).TotalMinutes, 1);
        Assert.Equal(stored.ExpiresAt, stored.NextAttemptAt);
        Assert.False(string.IsNullOrWhiteSpace(stored.ItemName));
    }

    [Fact]
    public async Task HandleAsync_WhenResponseHasNoSensitiveData_DoesNotScheduleCleanup()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();

        await ResetMessagesAsync(provider);

        var notifications = new StubNotificationService(SentMessageId);
        var response = Result<WorkflowResponse>.Success(new WorkflowResponse { Text = "Меню:" });

        using var scope = provider.CreateScope();

        var handler = new ServiceCallbackBotHandler(
            notifications,
            new StubMediator(response),
            scope.ServiceProvider.GetRequiredService<IUserErrorMapper>(),
            CreateScheduler(scope));

        await handler.HandleAsync(CreateCallbackUpdate(ChatId), CancellationToken.None);

        Assert.Single(notifications.SentMessages);

        using var verifyScope = provider.CreateScope();
        var context = verifyScope.ServiceProvider.GetRequiredService<PandoraDbContext>();

        Assert.Empty(await context.CredentialAccessMessage.AsNoTracking().ToListAsync());
    }

    private static Result<WorkflowResponse> CreateCredentialsResponse()
    {
        return Result<WorkflowResponse>.Success(new WorkflowResponse
        {
            Text = "Логин: test-login\nПароль: test-password",
            ContainsSensitiveData = true
        });
    }

    private static Update CreateCallbackUpdate(long chatId)
    {
        return new Update
        {
            CallbackQuery = new CallbackQuery
            {
                Data = CallbackConstants.ServicePrefix + ItemId,
                Message = new Message { Chat = new Chat { Id = chatId }, Id = 500 }
            }
        };
    }

    private static CredentialMessageScheduler CreateScheduler(IServiceScope scope)
    {
        return new CredentialMessageScheduler(
            scope.ServiceProvider.GetRequiredService<ICredentialAccessSettingsRepository>(),
            scope.ServiceProvider.GetRequiredService<ICredentialAccessMessageRepository>(),
            scope.ServiceProvider.GetRequiredService<ISecretMetadataRepository>(),
            new StubCredentialMessageGateway(),
            new CredentialAccessNoticeBuilder(scope.ServiceProvider.GetRequiredService<ILocalizer>()),
            NullLogger<CredentialMessageScheduler>.Instance);
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

    private sealed class StubMediator : IMediator
    {
        private readonly Result<WorkflowResponse> _response;

        public StubMediator(Result<WorkflowResponse> response)
        {
            _response = response;
        }

        public Task<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult((TResponse)(object)_response);
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
        {
            return Task.CompletedTask;
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<object?>(_response);
        }

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            return EmptyStream<TResponse>();
        }

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
        {
            return EmptyStream<object?>();
        }

        private static async IAsyncEnumerable<T> EmptyStream<T>()
        {
            await Task.CompletedTask;

            yield break;
        }

        public Task Publish<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            return Task.CompletedTask;
        }

        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class StubNotificationService : ITelegramNotificationService
    {
        private readonly int _messageId;

        public StubNotificationService(int messageId)
        {
            _messageId = messageId;
        }

        public List<string> SentMessages { get; } = new();

        public Task<Message> SendMessageAsync(
            long chatId,
            string text,
            ReplyMarkup? replyMarkup = null,
            CancellationToken cancellationToken = default)
        {
            SentMessages.Add(text);

            return Task.FromResult(new Message { Id = _messageId, Chat = new Chat { Id = chatId } });
        }

        public Task<Message> SendAlertAsync(
            long chatId,
            string text,
            ReplyMarkup? replyMarkup = null,
            CancellationToken cancellationToken = default)
        {
            return SendMessageAsync(chatId, text, replyMarkup, cancellationToken);
        }

        public Task<Message> EditMessageAsync(
            long chatId,
            int messageId,
            string text,
            InlineKeyboardMarkup? replyMarkup = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new Message { Id = messageId });
        }

        public Task DeleteMessageAsync(long chatId, int messageId, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class StubCredentialMessageGateway : ICredentialMessageGateway
    {
        public Task<CredentialMessageOutcome> Delete(
            long chatId,
            int messageId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(CredentialMessageOutcome.Succeeded);
        }

        public Task<CredentialMessageOutcome> ReplaceText(
            long chatId,
            int messageId,
            string text,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(CredentialMessageOutcome.Succeeded);
        }
    }
}
