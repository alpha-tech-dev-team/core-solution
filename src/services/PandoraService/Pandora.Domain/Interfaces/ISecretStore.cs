using Pandora.Domain.Models;

namespace Pandora.Domain.Interfaces;

public interface ISecretStore : ISecretCatalog
{
    Task<Secret?> GetSecret(string itemId, CancellationToken cancellationToken);
}
