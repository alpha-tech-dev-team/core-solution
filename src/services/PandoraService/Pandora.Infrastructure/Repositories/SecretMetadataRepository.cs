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

    public async Task<IReadOnlyList<SecretListItem>> GetItems(CancellationToken cancellationToken)
    {
        return await _context.SecretMetadata
            .OrderBy(metadata => metadata.ResourceName)
            .Select(metadata => new SecretListItem(metadata.VaultItemId, metadata.ResourceName, metadata.Username))
            .ToListAsync(cancellationToken);
    }

    public async Task<SecretMetadata?> Get(string vaultItemId, CancellationToken cancellationToken)
    {
        return await _context.SecretMetadata
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
