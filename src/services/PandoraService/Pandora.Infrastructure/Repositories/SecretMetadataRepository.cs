using Microsoft.EntityFrameworkCore;
using Pandora.Domain.Entities;
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

    public async Task<SecretListPage> GetPage(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.SecretMetadata.AsNoTracking();

        var total = await query.CountAsync(cancellationToken);
        var skip = (long)(page - 1) * pageSize;

        var items = await query
            .OrderBy(metadata => metadata.ResourceName)
            .ThenBy(metadata => metadata.VaultItemId)
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
            .FirstOrDefaultAsync(metadata => metadata.VaultItemId == vaultItemId, cancellationToken);
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
        await _context.SecretMetadata
            .Where(metadata => metadata.VaultItemId == vaultItemId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
