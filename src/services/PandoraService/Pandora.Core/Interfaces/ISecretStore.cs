using Pandora.Core.Models;

namespace Pandora.Core.Interfaces;

public interface ISecretStore
{
    Task<Secret?> GetSecret(string secretName, CancellationToken cancellationToken);

    IReadOnlyList<string> GetServiceNames();
}
