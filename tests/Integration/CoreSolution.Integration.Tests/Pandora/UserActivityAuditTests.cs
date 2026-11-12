using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Application.Messages;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pandora.TgBot.Application.Features.Credentials;
using Pandora.TgBot.Application.Features.Services;
using Pandora.Domain.Constants;
using Pandora.Domain.Entities;
using Pandora.Infrastructure.Database;
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
    public async Task SubmitPhone_WritesAuditEntryForServicesShownAfterActivation()
    {
        const long telegramUserId = 8104;
        const string phone = "+79995550114";

        await fixture.SeedPendingUserAsync(phone, "Pandora");
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        await fixture.SeedPandoraPermissionAsync(telegramUserId, ServiceName);

        var result = await SendAsync(provider, new SubmitPhoneMessage(telegramUserId, MessageId, phone));

        Assert.True(result.IsSuccess);

        var entries = await GetAuditEntriesAsync(provider, telegramUserId);

        Assert.Single(entries);
        Assert.Equal(AuditConstants.Actions.ShowServices, entries[0].Action);
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
