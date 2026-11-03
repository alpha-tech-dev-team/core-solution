using Pandora.Domain.Models;

namespace Pandora.Domain.Interfaces;

public interface ISecretCatalog
{
    Task<IReadOnlyList<SecretItem>> GetItems(CancellationToken cancellationToken);
}
