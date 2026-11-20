using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Constants;
using Identity.Domain.Enums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pandora.TgBot.Application.Features.Credentials;
using Xunit;

namespace CoreSolution.Integration.Tests.PandoraTests;

[Collection(nameof(IntegrationCollection))]
public sealed class IdentityUnavailableTests(IntegrationFixture fixture)
{
    private const int MessageId = 1;

    [Fact]
    public async Task UserWithoutSession_WhenIdentityIsRateLimited_GetsTemporaryUnavailableInsteadOfPhoneRequest()
    {
        const long telegramUserId = 8401;

        await using var provider = await fixture.CreatePandoraServiceProviderAsync(services =>
        {
            services.RemoveAll<IIdentityClient>();
            services.AddSingleton<IIdentityClient>(new RateLimitedIdentityClient());
        });

        using var scope = provider.CreateScope();

        await scope.ServiceProvider.GetRequiredService<ISessionService>()
            .DeleteSession(telegramUserId, CancellationToken.None);

        var result = await scope.ServiceProvider.GetRequiredService<IMediator>()
            .Send(new GetCredentialsMessage(telegramUserId, MessageId, "TestService"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.SessionTemporarilyUnavailable, result.ErrorCode);
    }

    private sealed class RateLimitedIdentityClient : IIdentityClient
    {
        public Task<string?> ActivateUser(long telegramUserId, string phone, CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>(ErrorCodes.ServiceTemporarilyUnavailable);
        }

        public Task<string?> CheckUser(long telegramUserId, string access, CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>(ErrorCodes.ServiceTemporarilyUnavailable);
        }
    }
}
