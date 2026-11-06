using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreSolution.Integration.Tests.Shared;

[Collection(nameof(IntegrationCollection))]
public sealed class SessionIsolationTests(IntegrationFixture fixture)
{
    [Fact]
    public async Task Sessions_AreNotSharedBetweenBots()
    {
        const long telegramUserId = 8100;

        await using var atlas = fixture.CreateAtlasServiceProvider();
        await using var pandora = await fixture.CreatePandoraServiceProviderAsync();

        await SaveSessionAsync(atlas, telegramUserId);

        Assert.NotNull(await GetSessionAsync(atlas, telegramUserId));
        Assert.Null(await GetSessionAsync(pandora, telegramUserId));
    }

    private static async Task SaveSessionAsync(IServiceProvider provider, long telegramUserId)
    {
        using var scope = provider.CreateScope();

        await scope.ServiceProvider.GetRequiredService<ISessionService>()
            .SaveSession(UserSession.Create(telegramUserId, "ProblemCategorySelection"), CancellationToken.None);
    }

    private static async Task<UserSession?> GetSessionAsync(IServiceProvider provider, long telegramUserId)
    {
        using var scope = provider.CreateScope();

        return await scope.ServiceProvider.GetRequiredService<ISessionService>()
            .GetSession(telegramUserId, CancellationToken.None);
    }
}
