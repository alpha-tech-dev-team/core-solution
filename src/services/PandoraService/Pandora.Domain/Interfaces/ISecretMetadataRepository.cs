using Pandora.Domain.Entities;
using Pandora.Domain.Models;

namespace Pandora.Domain.Interfaces;

public interface ISecretMetadataRepository
{
    Task<SecretListPage> GetPage(
        int page,
        int pageSize,
        Func<IQueryable<SecretMetadata>, IOrderedQueryable<SecretMetadata>> orderBy,
        CancellationToken cancellationToken);
    Task<SecretMetadata?> GetById(string vaultItemId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SecretMetadata>> GetPendingSync(CancellationToken cancellationToken);
    Task<bool> ClearSyncStateIfUnchanged(string vaultItemId, string resourceName, string username, CancellationToken cancellationToken);
    Task<bool> DeleteIfPending(string vaultItemId, CancellationToken cancellationToken);
    Task Add(SecretMetadata metadata, CancellationToken cancellationToken);
    Task Update(SecretMetadata metadata, CancellationToken cancellationToken);
    Task Delete(string vaultItemId, CancellationToken cancellationToken);
}
