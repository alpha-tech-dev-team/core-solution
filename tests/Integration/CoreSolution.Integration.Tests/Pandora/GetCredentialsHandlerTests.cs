using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Enums;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using Microsoft.Extensions.Logging.Abstractions;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;
using Pandora.TgBot.Application.Features.Credentials;
using Xunit;

namespace CoreSolution.Integration.Tests.PandoraTests;

public sealed class GetCredentialsHandlerTests
{
    [Fact]
    public async Task Handle_WhenPasswordIsNotAtEndOfTemplate_MasksItsActualPosition()
    {
        const string password = "secret-password";
        const string template = "{0}\n{1}: {2}\n{3}: {4}\nPostfix";
        var localizer = new StubLocalizer(template);
        var handler = new GetCredentialsHandler(
            new StubSecretStore(new Secret("Service", "login", password)),
            new StubPermissionRepository(),
            localizer,
            NullLogger<GetCredentialsHandler>.Instance);

        var result = await handler.Handle(
            new GetCredentialsMessage(7001, 33, "service-id"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.EndsWith("Postfix", result.Value!.Text);
        var mask = Assert.Single(result.Value.TextMasks!);
        var actualPasswordOffset = result.Value.Text.IndexOf(password, StringComparison.Ordinal);

        Assert.True(actualPasswordOffset >= 0);
        Assert.Equal(actualPasswordOffset, mask.Offset);
        Assert.Equal(password.Length, mask.Length);
    }

    private sealed class StubLocalizer(string credentialsTemplate) : ILocalizer
    {
        public string GetPhrase(string state, string key)
        {
            return key switch
            {
                "CredentialsTemplate" => credentialsTemplate,
                "LoginLabel" => "Login",
                "PasswordLabel" => "Password",
                _ => key
            };
        }

        public string GetPhrase(string state, string key, Language language)
        {
            return GetPhrase(state, key);
        }
    }

    private sealed class StubSecretStore(Secret secret) : ISecretStore
    {
        public Task<Result<Secret>> GetSecret(string itemId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<Secret>.Success(secret));
        }

        public Task<Result<IReadOnlyList<SecretItem>>> GetItems(CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<IReadOnlyList<SecretItem>>.Success(Array.Empty<SecretItem>()));
        }
    }

    private sealed class StubPermissionRepository : IPermissionRepository
    {
        public Task<IReadOnlyList<string>> GetAllowedItemIds(long telegramUserId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }

        public Task<bool> HasAccess(long telegramUserId, string itemId, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task<EmployeeSecretPage> GetPage(long telegramUserId, SecretPageMessage message, CancellationToken cancellationToken)
        {
            return Task.FromResult(new EmployeeSecretPage(Array.Empty<EmployeeSecretItem>(), 0, 1, 1));
        }

        public Task<SecretAccessGrantResult> Grant(long telegramUserId, string itemId, Guid adminUserId, CancellationToken cancellationToken)
        {
            return Task.FromResult(SecretAccessGrantResult.Granted);
        }

        public Task<SecretAccessRevokeResult> Revoke(long telegramUserId, string itemId, CancellationToken cancellationToken)
        {
            return Task.FromResult(SecretAccessRevokeResult.Revoked);
        }

        public Task SaveChanges(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
