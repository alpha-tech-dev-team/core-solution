using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Enums;
using CoreSolution.Shared.Application.Messages;

using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using Identity.Domain.Enums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Xunit;

namespace CoreSolution.Integration.Tests.PandoraTests;

[Collection(nameof(IntegrationCollection))]
public sealed class SubmitPhoneFlowTests(IntegrationFixture fixture)
{
    private const int MessageId = 1;

    [Fact]
    public async Task SubmitPhone_ActivatesUserInIdentityAndSavesSessionInRedis()
    {
        const long telegramUserId = 777;
        const string phone = "+79991234567";

        await fixture.SeedPendingUserAsync(phone);
        await using var provider = fixture.CreateAtlasServiceProvider();

        var result = await SendAsync(provider, telegramUserId, "+7 (999) 123-45-67");

        Assert.True(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.Value!.Text));
        Assert.DoesNotContain("[", result.Value.Text);

        var state = await fixture.GetUserStateAsync(phone);
        Assert.Equal(telegramUserId, state.TelegramUserId);
        Assert.Equal(UserStatus.Active, state.Status);

        using var scope = provider.CreateScope();
        var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
        var session = await sessionService.GetSession(telegramUserId, CancellationToken.None);

        Assert.NotNull(session);
        Assert.Equal(nameof(Atlas.Domain.Enums.WorkflowStep.ProblemCategorySelection), session.State);
        Assert.Equal(Language.Ru, session.Language);

        var timeToLive = await scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>()
            .GetDatabase()
            .KeyTimeToLiveAsync($"session:{telegramUserId}");

        Assert.NotNull(timeToLive);
        Assert.True(timeToLive > TimeSpan.Zero);
    }

    [Fact]
    public async Task SubmitPhone_UnknownPhone_ReturnsAccessNotConfirmedAndSavesNoSession()
    {
        const long telegramUserId = 778;

        await using var provider = fixture.CreateAtlasServiceProvider();

        var result = await SendAsync(provider, telegramUserId, "+79990000000");

        Assert.False(result.IsSuccess);
        Assert.Equal("AccessNotConfirmed", result.ErrorCode);

        using var scope = provider.CreateScope();
        var session = await scope.ServiceProvider.GetRequiredService<ISessionService>()
            .GetSession(telegramUserId, CancellationToken.None);

        Assert.Null(session);
    }

    [Fact]
    public async Task SubmitPhone_SecondCallForSameUser_StaysSuccessful()
    {
        const long telegramUserId = 779;
        const string phone = "+79991234568";

        await fixture.SeedPendingUserAsync(phone);
        await using var provider = fixture.CreateAtlasServiceProvider();

        var first = await SendAsync(provider, telegramUserId, "+7 (999) 123-45-68");
        var second = await SendAsync(provider, telegramUserId, "+7 (999) 123-45-68");

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
    }

    [Fact]
    public async Task SubmitPhone_WithoutPhone_RequestsContact()
    {
        await using var provider = fixture.CreateAtlasServiceProvider();

        var result = await SendAsync(provider, 780, null);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.RequestContact);
        Assert.Equal("Отправьте номер телефона", result.Value.Text);
    }

    private static async Task<Result<WorkflowResponse>> SendAsync(
        IServiceProvider provider,
        long telegramUserId,
        string? phone)
    {
        using var scope = provider.CreateScope();

        return await scope.ServiceProvider.GetRequiredService<IMediator>()
            .Send(new SubmitPhoneMessage(telegramUserId, MessageId, phone), CancellationToken.None);
    }
}
