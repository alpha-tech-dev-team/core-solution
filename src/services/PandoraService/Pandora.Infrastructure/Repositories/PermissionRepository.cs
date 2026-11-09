using Microsoft.EntityFrameworkCore;
using Pandora.Domain.Entities;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;
using Pandora.Infrastructure.Database;

namespace Pandora.Infrastructure.Repositories;

public sealed class PermissionRepository : IPermissionRepository
{
    private readonly PandoraDbContext _context;

    public PermissionRepository(PandoraDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<string>> GetAllowedItemIds(long telegramUserId, CancellationToken cancellationToken)
    {
        return await _context.Permission
            .Where(permission => permission.TelegramUserId == telegramUserId && permission.IsActive)
            .Select(permission => permission.VaultItemId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasAccess(long telegramUserId, string itemId, CancellationToken cancellationToken)
    {
        return await _context.Permission
            .AnyAsync(
                permission => permission.TelegramUserId == telegramUserId
                    && permission.VaultItemId == itemId
                    && permission.IsActive,
                cancellationToken);
    }

    public async Task<EmployeeSecretPage> GetPage(long telegramUserId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.SecretMetadata.AsNoTracking();

        var total = await query.CountAsync(cancellationToken);
        var skip = (long)(page - 1) * pageSize;

        var items = await query
            .OrderBy(metadata => metadata.ResourceName)
            .ThenBy(metadata => metadata.VaultItemId)
            .Skip((int)Math.Min(skip, int.MaxValue))
            .Take(pageSize)
            .Select(metadata => new EmployeeSecretItem(
                metadata.VaultItemId,
                metadata.ResourceName,
                metadata.Username,
                _context.Permission.Any(permission => permission.TelegramUserId == telegramUserId
                    && permission.VaultItemId == metadata.VaultItemId
                    && permission.IsActive)))
            .ToListAsync(cancellationToken);

        return new EmployeeSecretPage(items, page, pageSize, total);
    }

    public async Task<bool> Grant(long telegramUserId, string itemId, CancellationToken cancellationToken)
    {
        var secretExists = await _context.SecretMetadata
            .AnyAsync(metadata => metadata.VaultItemId == itemId, cancellationToken);

        if (!secretExists)
        {
            return false;
        }

        var permission = await _context.Permission
            .FirstOrDefaultAsync(
                item => item.TelegramUserId == telegramUserId && item.VaultItemId == itemId,
                cancellationToken);

        if (permission is null)
        {
            _context.Permission.Add(new Permission
            {
                TelegramUserId = telegramUserId,
                VaultItemId = itemId,
                IsActive = true
            });

            return true;
        }

        if (permission.IsActive)
        {
            return true;
        }

        permission.IsActive = true;

        return true;
    }

    public async Task<bool> Revoke(long telegramUserId, string itemId, CancellationToken cancellationToken)
    {
        var secretExists = await _context.SecretMetadata
            .AnyAsync(metadata => metadata.VaultItemId == itemId, cancellationToken);

        if (!secretExists)
        {
            return false;
        }

        var permission = await _context.Permission
            .FirstOrDefaultAsync(
                item => item.TelegramUserId == telegramUserId && item.VaultItemId == itemId,
                cancellationToken);

        if (permission is null || !permission.IsActive)
        {
            return true;
        }

        permission.IsActive = false;

        return true;
    }

    public Task SaveChanges(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
