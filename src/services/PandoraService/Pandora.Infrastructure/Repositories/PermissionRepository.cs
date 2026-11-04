using Microsoft.EntityFrameworkCore;
using Pandora.Domain.Interfaces;
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
}
