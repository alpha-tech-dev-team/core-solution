using Microsoft.EntityFrameworkCore;
using Pandora.Domain.Entities;
using Pandora.Domain.Enums;
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

    public async Task<EmployeeSecretPage> GetPage(
        long telegramUserId,
        SecretPageMessage message,
        CancellationToken cancellationToken)
    {
        var source = LiveSecrets();
        var total = await source.CountAsync(cancellationToken);

        var items = await message.OrderBy(source)
            .Skip(message.Page.Skip)
            .Take(message.Page.PageSize)
            .Select(metadata => new EmployeeSecretItem(
                metadata.VaultItemId,
                metadata.ResourceName,
                metadata.Username,
                _context.Permission.Any(permission => permission.TelegramUserId == telegramUserId
                    && permission.VaultItemId == metadata.VaultItemId
                    && permission.IsActive)))
            .ToListAsync(cancellationToken);

        return new EmployeeSecretPage(items, message.Page.Page, message.Page.PageSize, total);
    }

    public async Task<SecretAccessGrantResult> Grant(
        long telegramUserId,
        string itemId,
        Guid adminUserId,
        CancellationToken cancellationToken)
    {
        if (!await LiveSecrets().AnyAsync(metadata => metadata.VaultItemId == itemId, cancellationToken))
        {
            return SecretAccessGrantResult.SecretNotFound;
        }

        var permission = await _context.Permission
            .FirstOrDefaultAsync(
                item => item.TelegramUserId == telegramUserId && item.VaultItemId == itemId,
                cancellationToken);

        if (permission is not null && permission.IsActive)
        {
            return SecretAccessGrantResult.AlreadyGranted;
        }

        var grantedAt = DateTime.UtcNow;

        if (permission is null)
        {
            _context.Permission.Add(new Permission
            {
                TelegramUserId = telegramUserId,
                VaultItemId = itemId,
                IsActive = true,
                GrantedAt = grantedAt,
                GrantedBy = adminUserId
            });

            return SecretAccessGrantResult.Granted;
        }

        permission.IsActive = true;
        permission.GrantedAt = grantedAt;
        permission.GrantedBy = adminUserId;

        return SecretAccessGrantResult.Granted;
    }

    public async Task<SecretAccessRevokeResult> Revoke(
        long telegramUserId,
        string itemId,
        CancellationToken cancellationToken)
    {
        if (!await LiveSecrets().AnyAsync(metadata => metadata.VaultItemId == itemId, cancellationToken))
        {
            return SecretAccessRevokeResult.SecretNotFound;
        }

        var permission = await _context.Permission
            .FirstOrDefaultAsync(
                item => item.TelegramUserId == telegramUserId && item.VaultItemId == itemId,
                cancellationToken);

        if (permission is null || !permission.IsActive)
        {
            return SecretAccessRevokeResult.AlreadyRevoked;
        }

        permission.IsActive = false;

        return SecretAccessRevokeResult.Revoked;
    }

    public Task SaveChanges(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<SecretMetadata> LiveSecrets()
    {
        return _context.SecretMetadata
            .AsNoTracking()
            .Where(metadata => metadata.SyncOperation != SecretSyncOperation.Delete);
    }
}
