using Microsoft.Extensions.Options;
using Pandora.Core.Interfaces;
using Pandora.Core.Models;

namespace Pandora.Infrastructure.Secrets;

public sealed class ConfigSecretStore : ISecretStore
{
    private readonly IOptionsMonitor<SecretsOptions> _options;

    public ConfigSecretStore(IOptionsMonitor<SecretsOptions> options)
    {
        _options = options;
    }

    public Task<Secret?> GetSecretAsync(string secretName, CancellationToken cancellationToken)
    {
        var item = _options.CurrentValue.Items.FirstOrDefault(candidate => candidate.Name == secretName);
        Secret? secret = item is null ? null : new Secret(item.Login, item.Password);

        return Task.FromResult(secret);
    }
}
