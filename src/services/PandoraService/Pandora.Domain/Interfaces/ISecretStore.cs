using Pandora.Domain.Models;

namespace Pandora.Domain.Interfaces;

public interface ISecretStore
{
    Task<Secret?> GetSecret(string secretName, CancellationToken cancellationToken);
    IReadOnlyList<string> GetServiceNames();
}
