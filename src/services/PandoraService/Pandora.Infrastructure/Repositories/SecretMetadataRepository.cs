using Microsoft.EntityFrameworkCore;
using Pandora.Domain.Constants;
using Pandora.Domain.Entities;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;
using Pandora.Infrastructure.Database;

namespace Pandora.Infrastructure.Repositories;

public sealed class SecretMetadataRepository : ISecretMetadataRepository
{
    private readonly PandoraDbContext _context;

    public SecretMetadataRepository(PandoraDbContext context)
    {
        _context = context;
    }

    public async Task<SecretListPage> GetPage(SecretPageQuery query, CancellationToken cancellationToken)
    {
        var source = _context.SecretMetadata
            .AsNoTracking()
            .Where(metadata => metadata.SyncOperation != SecretSyncOperation.Delete);

        var total = await source.CountAsync(cancellationToken);
        var skip = (long)(query.Page - PaginationLimits.FirstPage) * query.PageSize;

        var items = await query.OrderBy(source)
            .Skip((int)Math.Min(skip, int.MaxValue))
            .Take(query.PageSize)
            .Select(metadata => new SecretListItem(metadata.VaultItemId, metadata.ResourceName, metadata.Username))
            .ToListAsync(cancellationToken);

        return new SecretListPage(items, query.Page, query.PageSize, total);
    }

    public async Task<SecretMetadata?> GetById(string vaultItemId, CancellationToken cancellationToken)
    {
        return await _context.SecretMetadata
            .AsNoTracking()
            .FirstOrDefaultAsync(
                metadata => metadata.VaultItemId == vaultItemId
                    && metadata.SyncOperation != SecretSyncOperation.Delete,
                cancellationToken);
    }

    public async Task<IReadOnlyList<SecretMetadata>> GetPendingSync(SecretSyncBatchQuery query, CancellationToken cancellationToken)
    {
        return await _context.SecretMetadata
            .AsNoTracking()
            .Where(metadata => metadata.SyncOperation != SecretSyncOperation.None
                && metadata.AttemptCount < query.MaxAttempts
                && (metadata.NextAttemptAt == null || metadata.NextAttemptAt <= query.UtcNow))
            .OrderBy(metadata => metadata.AttemptCount == 0 ? 0 : 1)
            .ThenBy(metadata => metadata.NextAttemptAt)
            .Take(query.BatchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ClearSyncStateIfUnchanged(
        string vaultItemId,
        string resourceName,
        string username,
        CancellationToken cancellationToken)
    {
        var updated = await _context.SecretMetadata
            .Where(metadata => metadata.VaultItemId == vaultItemId
                && metadata.SyncOperation == SecretSyncOperation.Update
                && metadata.ResourceName == resourceName
                && metadata.Username == username)
            .ExecuteUpdateAsync(update => update
                .SetProperty(metadata => metadata.SyncOperation, SecretSyncOperation.None)
                .SetProperty(metadata => metadata.AttemptCount, 0)
                .SetProperty(metadata => metadata.NextAttemptAt, (DateTime?)null)
                .SetProperty(metadata => metadata.LastError, (string?)null),
                cancellationToken);

        return updated > 0;
    }

    public async Task<bool> MarkSyncAttemptFailed(
        string vaultItemId,
        string error,
        DateTime nextAttemptAt,
        CancellationToken cancellationToken)
    {
        var updated = await _context.SecretMetadata
            .Where(metadata => metadata.VaultItemId == vaultItemId
                && metadata.SyncOperation != SecretSyncOperation.None)
            .ExecuteUpdateAsync(update => update
                .SetProperty(metadata => metadata.AttemptCount, metadata => metadata.AttemptCount + 1)
                .SetProperty(metadata => metadata.NextAttemptAt, nextAttemptAt)
                .SetProperty(metadata => metadata.LastError, error),
                cancellationToken);

        return updated > 0;
    }

    public async Task<bool> DeleteIfPending(string vaultItemId, CancellationToken cancellationToken)
    {
        var deleted = await _context.SecretMetadata
            .Where(metadata => metadata.VaultItemId == vaultItemId
                && metadata.SyncOperation == SecretSyncOperation.Delete)
            .ExecuteDeleteAsync(cancellationToken);

        return deleted > 0;
    }

    public async Task Add(SecretMetadata metadata, CancellationToken cancellationToken)
    {
        _context.SecretMetadata.Add(metadata);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task Update(SecretMetadata metadata, CancellationToken cancellationToken)
    {
        _context.SecretMetadata.Update(metadata);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task Delete(string vaultItemId, CancellationToken cancellationToken)
    {
        var metadata = await _context.SecretMetadata
            .FirstOrDefaultAsync(metadata => metadata.VaultItemId == vaultItemId, cancellationToken);

        if (metadata is null)
        {
            return;
        }

        _context.SecretMetadata.Remove(metadata);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
