using Pandora.Domain.Entities;
using Pandora.Domain.Models;

namespace Pandora.Domain.Interfaces;

public interface ISecretMetadataRepository
{
    Task<IReadOnlyList<SecretListItem>> GetItems(CancellationToken cancellationToken);
    Task<SecretMetadata?> Get(string vaultItemId, CancellationToken cancellationToken);
    Task Add(SecretMetadata metadata, CancellationToken cancellationToken);
    Task Update(SecretMetadata metadata, CancellationToken cancellationToken);
    Task Delete(string vaultItemId, CancellationToken cancellationToken);
}
