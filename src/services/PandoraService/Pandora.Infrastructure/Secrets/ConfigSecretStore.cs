using CoreSolution.Shared.Results;
using Microsoft.Extensions.Options;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;
using Pandora.Domain.Options;

namespace Pandora.Infrastructure.Secrets;

public sealed class ConfigSecretStore : ISecretStore
{
    private readonly IOptionsMonitor<SecretsOptions> _options;

    public ConfigSecretStore(IOptionsMonitor<SecretsOptions> options)
    {
        _options = options;
    }

    public Task<Result<Secret>> GetSecret(string itemId, CancellationToken cancellationToken)
    {
        var item = _options.CurrentValue.Items.FirstOrDefault(candidate => candidate.Name == itemId);

        if (item is null)
        {
            return Task.FromResult(Result<Secret>.Failure(ErrorCode.SecretNotFound.ToString()));
        }

        return Task.FromResult(Result<Secret>.Success(new Secret(item.Name, item.Login, item.Password)));
    }

    public Task<Result<IReadOnlyList<SecretItem>>> GetItems(CancellationToken cancellationToken)
    {
        IReadOnlyList<SecretItem> items = _options.CurrentValue.Items
            .Select(item => new SecretItem(item.Name, item.Name))
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<SecretItem>>.Success(items));
    }
}
