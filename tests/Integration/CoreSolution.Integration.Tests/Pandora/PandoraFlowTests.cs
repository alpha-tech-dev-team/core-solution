using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Application.Messages;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Pandora.TgBot.Features.Credentials;
using Xunit;

namespace CoreSolution.Integration.Tests.PandoraTests;

[Collection(nameof(IntegrationCollection))]
public sealed class PandoraFlowTests(IntegrationFixture fixture)
{
    private const int MessageId = 1;

    [Fact]
    public async Task SubmitPhone_ActivatesUserShowsServicesAndSavesSessionInRedis()
    {
        const long telegramUserId = 8001;
        const string phone = "+79995550101";

        await fixture.SeedPendingUserAsync(phone, "Pandora");
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        await fixture.SeedPandoraPermissionAsync(telegramUserId, "TestService");

        var result = await SendSubmitPhoneAsync(provider, telegramUserId, "+7 (999) 555-01-01");

        Assert.True(result.IsSuccess);
        Assert.Equal("Доступные сервисы:", result.Value!.Text);
        Assert.NotNull(result.Value.ButtonRows);

        var state = await fixture.GetUserStateAsync(phone);
        Assert.Equal(telegramUserId, state.TelegramUserId);

        var session = await GetSessionAsync(provider, telegramUserId);
        Assert.NotNull(session);
        Assert.Equal(nameof(Pandora.Domain.Enums.WorkflowStep.ServiceSelection), session!.State);
    }

    [Fact]
    public async Task GetCredentials_AfterSessionLoss_RechecksIdentityAndRestoresSession()
    {
        const long telegramUserId = 8002;
        const string phone = "+79995550102";

        await fixture.SeedPendingUserAsync(phone, "Pandora");
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        await fixture.SeedPandoraPermissionAsync(telegramUserId, "TestService");
        _ = await SendSubmitPhoneAsync(provider, telegramUserId, "+7 (999) 555-01-02");

        using (var scope = provider.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<ISessionService>()
                .DeleteSession(telegramUserId, CancellationToken.None);
        }

        var result = await SendGetCredentialsAsync(provider, telegramUserId, "TestService");

        Assert.True(result.IsSuccess);
        Assert.Contains("test-login", result.Value!.Text);
        Assert.Contains("test-password", result.Value.Text);

        var session = await GetSessionAsync(provider, telegramUserId);
        Assert.NotNull(session);
        Assert.Equal(nameof(Pandora.Domain.Enums.WorkflowStep.ServiceSelection), session!.State);
    }

    [Fact]
    public async Task GetCredentials_UserWithoutPandoraRole_RequestsPhoneAndSavesNoSession()
    {
        const long telegramUserId = 8003;
        const string phone = "+79995550103";

        await fixture.SeedPendingUserAsync(phone, "Atlas");
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        _ = await SendSubmitPhoneAsync(provider, telegramUserId, "+7 (999) 555-01-03");

        using (var scope = provider.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<ISessionService>()
                .DeleteSession(telegramUserId, CancellationToken.None);
        }

        var result = await SendGetCredentialsAsync(provider, telegramUserId, "TestService");

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.RequestContact);
        Assert.Equal("Отправьте номер телефона", result.Value.Text);

        Assert.Null(await GetSessionAsync(provider, telegramUserId));
    }

    private static async Task<Result<WorkflowResponse>> SendSubmitPhoneAsync(
        IServiceProvider provider,
        long telegramUserId,
        string phone)
    {
        using var scope = provider.CreateScope();

        return await scope.ServiceProvider.GetRequiredService<IMediator>()
            .Send(new SubmitPhoneMessage(telegramUserId, MessageId, phone), CancellationToken.None);
    }

    private static async Task<Result<WorkflowResponse>> SendGetCredentialsAsync(
        IServiceProvider provider,
        long telegramUserId,
        string serviceName)
    {
        using var scope = provider.CreateScope();

        return await scope.ServiceProvider.GetRequiredService<IMediator>()
            .Send(new GetCredentialsMessage(telegramUserId, MessageId, serviceName), CancellationToken.None);
    }

    private static async Task<CoreSolution.Shared.Models.UserSession?> GetSessionAsync(
        IServiceProvider provider,
        long telegramUserId)
    {
        using var scope = provider.CreateScope();

        return await scope.ServiceProvider.GetRequiredService<ISessionService>()
            .GetSession(telegramUserId, CancellationToken.None);
    }
}
