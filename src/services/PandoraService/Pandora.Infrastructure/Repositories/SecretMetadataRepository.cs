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

    public async Task<SecretListPage> GetPage(
        int page,
        int pageSize,
        Func<IQueryable<SecretMetadata>, IOrderedQueryable<SecretMetadata>> orderBy,
        CancellationToken cancellationToken)
    {
        var query = _context.SecretMetadata
            .AsNoTracking()
            .Where(metadata => metadata.SyncOperation != SecretSyncOperation.Delete);

        var total = await query.CountAsync(cancellationToken);
        var skip = (long)(page - PaginationLimits.FirstPage) * pageSize;

        var items = await orderBy(query)
            .Skip((int)Math.Min(skip, int.MaxValue))
            .Take(pageSize)
            .Select(metadata => new SecretListItem(metadata.VaultItemId, metadata.ResourceName, metadata.Username))
            .ToListAsync(cancellationToken);

        return new SecretListPage(items, page, pageSize, total);
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

    public async Task<IReadOnlyList<SecretMetadata>> GetPendingSync(CancellationToken cancellationToken)
    {
        return await _context.SecretMetadata
            .Where(metadata => metadata.SyncOperation != SecretSyncOperation.None)
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
                .SetProperty(metadata => metadata.SyncOperation, SecretSyncOperation.None),
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
