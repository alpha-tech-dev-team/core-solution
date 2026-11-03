using Microsoft.Extensions.Options;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;

namespace Pandora.Infrastructure.Secrets;

public sealed class ConfigSecretStore : ISecretStore
{
    private readonly IOptionsMonitor<SecretsOptions> _options;

    public ConfigSecretStore(IOptionsMonitor<SecretsOptions> options)
    {
        _options = options;
    }

    public Task<Secret?> GetSecret(string itemId, CancellationToken cancellationToken)
    {
        var item = _options.CurrentValue.Items.FirstOrDefault(candidate => candidate.Name == itemId);
        Secret? secret = item is null ? null : new Secret(item.Name, item.Login, item.Password);

        return Task.FromResult(secret);
    }

    public Task<IReadOnlyList<SecretItem>> GetItems(CancellationToken cancellationToken)
    {
        IReadOnlyList<SecretItem> items = _options.CurrentValue.Items
            .Select(item => new SecretItem(item.Name, item.Name))
            .ToList();

        return Task.FromResult(items);
    }
}
