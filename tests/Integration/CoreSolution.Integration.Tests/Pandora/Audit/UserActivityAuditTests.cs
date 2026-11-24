using CoreSolution.Shared.Application.Messages;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Pandora.Domain.Constants;
using Pandora.Domain.Entities;
using Pandora.Infrastructure.Audit;
using Pandora.Infrastructure.Database;
using Pandora.TgBot.Application.Common.Services;
using Pandora.TgBot.Application.Features.Credentials;
using Pandora.TgBot.Application.Features.Services;
using Xunit;

namespace CoreSolution.Integration.Tests.PandoraTests;

[Collection(nameof(IntegrationCollection))]
public sealed class UserActivityAuditTests(IntegrationFixture fixture)
{
    private const int MessageId = 1;
    private const string ServiceName = "TestService";

    [Fact]
    public async Task GetCredentials_Success_WritesAuditEntryWithUsersActionAndTarget()
    {
        const long telegramUserId = 8101;
        const string phone = "+79995550111";

        await fixture.SeedPendingUserAsync(phone, "Pandora");
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        await fixture.SeedPandoraPermissionAsync(telegramUserId, ServiceName);
        _ = await SendAsync(provider, new SubmitPhoneMessage(telegramUserId, MessageId, phone));

        var result = await SendAsync(provider, new GetCredentialsMessage(telegramUserId, MessageId, ServiceName));

        Assert.True(result.IsSuccess);

        var entries = await GetAuditEntriesAsync(provider, telegramUserId);
        var credentialEntry = Assert.Single(entries, entry => entry.Action == AuditConstants.Actions.GetCredentials);

        Assert.Equal(telegramUserId, credentialEntry.TelegramUserId);
        Assert.Equal(ServiceName, credentialEntry.TargetItemId);
        Assert.Equal(AuditConstants.Sources.Telegram, credentialEntry.RequestSource);
        Assert.NotEqual(default, credentialEntry.Timestamp);
    }

    [Fact]
    public async Task GetCredentials_AccessDenied_WritesNoAuditEntry()
    {
        const long telegramUserId = 8102;
        const string phone = "+79995550112";

        await fixture.SeedPendingUserAsync(phone, "Pandora");
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        _ = await SendAsync(provider, new SubmitPhoneMessage(telegramUserId, MessageId, phone));

        var result = await SendAsync(provider, new GetCredentialsMessage(telegramUserId, MessageId, ServiceName));

        Assert.False(result.IsSuccess);

        var entries = await GetAuditEntriesAsync(provider, telegramUserId);

        Assert.DoesNotContain(entries, entry => entry.Action == AuditConstants.Actions.GetCredentials);
    }

    [Fact]
    public async Task GetCredentials_WithoutSession_RequestsPhoneAndWritesNoAuditEntry()
    {
        const long telegramUserId = 8105;

        await using var provider = await fixture.CreatePandoraServiceProviderAsync();

        var result = await SendAsync(provider, new GetCredentialsMessage(telegramUserId, MessageId, ServiceName));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.RequestContact);

        var entries = await GetAuditEntriesAsync(provider, telegramUserId);

        Assert.Empty(entries);
    }

    [Fact]
    public async Task ShowServices_Success_WritesAuditEntryWithoutTarget()
    {
        const long telegramUserId = 8103;

        await using var provider = await fixture.CreatePandoraServiceProviderAsync();

        var result = await SendAsync(provider, new ShowServicesMessage(telegramUserId, MessageId));

        Assert.True(result.IsSuccess);

        var entries = await GetAuditEntriesAsync(provider, telegramUserId);

        Assert.Single(entries);
        Assert.Equal(AuditConstants.Actions.ShowServices, entries[0].Action);
        Assert.Null(entries[0].TargetItemId);
    }

    [Fact]
    public async Task SubmitPhone_WritesPhoneSharingAndServicesShownEntries()
    {
        const long telegramUserId = 8104;
        const string phone = "+79995550114";

        await fixture.SeedPendingUserAsync(phone, "Pandora");
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        await fixture.SeedPandoraPermissionAsync(telegramUserId, ServiceName);

        var result = await SendAsync(provider, new SubmitPhoneMessage(telegramUserId, MessageId, phone));

        Assert.True(result.IsSuccess);

        var entries = await GetAuditEntriesAsync(provider, telegramUserId);
        var submitPhoneEntry = Assert.Single(entries, entry => entry.Action == AuditConstants.Actions.SubmitPhone);

        Assert.Null(submitPhoneEntry.TargetItemId);
        Assert.Equal(AuditConstants.Sources.Telegram, submitPhoneEntry.RequestSource);
        Assert.Contains(entries, entry => entry.Action == AuditConstants.Actions.ShowServices);
    }

    [Fact]
    public async Task SubmitPhone_UnknownPhone_WritesNoPhoneSharingEntry()
    {
        const long telegramUserId = 8107;

        await using var provider = await fixture.CreatePandoraServiceProviderAsync();

        var result = await SendAsync(provider, new SubmitPhoneMessage(telegramUserId, MessageId, "+79995550199"));

        Assert.False(result.IsSuccess);

        var entries = await GetAuditEntriesAsync(provider, telegramUserId);

        Assert.DoesNotContain(entries, entry => entry.Action == AuditConstants.Actions.SubmitPhone);
    }

    [Fact]
    public async Task ShowServices_AuditStorageUnavailable_StillReturnsServices()
    {
        const long telegramUserId = 8106;

        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        await RenameAuditTableAsync("audit_unavailable");

        try
        {
            var result = await SendAsync(provider, new ShowServicesMessage(telegramUserId, MessageId));

            Assert.True(result.IsSuccess);
            Assert.False(string.IsNullOrWhiteSpace(result.Value!.Text));
        }
        finally
        {
            await RenameAuditTableAsync("audit");
        }
    }

    [Fact]
    public async Task Log_WritesAuditEntryAndOutboxEntryInSameSave()
    {
        const long telegramUserId = 8109;
        const string action = "telegram_outbox_test";
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = provider.CreateScope();
        var auditLogger = scope.ServiceProvider.GetRequiredService<UserActivityAuditLogger>();

        await auditLogger.Log(telegramUserId, action, "target-item", CancellationToken.None);

        var database = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();
        var entry = Assert.Single(await database.AuditEntry.AsNoTracking()
            .Where(item => item.TelegramUserId == telegramUserId)
            .ToListAsync());
        var outbox = Assert.Single(await database.AuditOutbox.AsNoTracking()
            .Where(item => item.Payload.Contains(entry.Id.ToString()))
            .ToListAsync());
        var notification = AuditNotificationSerializer.Deserialize(outbox.Payload);

        Assert.Equal(action, entry.Action);
        Assert.NotNull(notification);
        Assert.Equal(entry.Id, notification.Id);
        Assert.Equal(telegramUserId, notification.TelegramUserId);
    }

    private async Task RenameAuditTableAsync(string targetName)
    {
        await using var connection = new NpgsqlConnection(fixture.PandoraConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = targetName == "audit"
            ? "alter table audit_unavailable rename to audit"
            : $"alter table audit rename to {targetName}";

        await command.ExecuteNonQueryAsync();
    }

    private static async Task<Result<WorkflowResponse>> SendAsync<TRequest>(IServiceProvider provider, TRequest request)
        where TRequest : IRequest<Result<WorkflowResponse>>
    {
        using var scope = provider.CreateScope();

        return await scope.ServiceProvider.GetRequiredService<IMediator>()
            .Send(request, CancellationToken.None);
    }

    private static async Task<IReadOnlyList<AuditEntry>> GetAuditEntriesAsync(IServiceProvider provider, long telegramUserId)
    {
        using var scope = provider.CreateScope();

        return await scope.ServiceProvider.GetRequiredService<PandoraDbContext>().AuditEntry
            .Where(entry => entry.TelegramUserId == telegramUserId)
            .ToListAsync();
    }
}
