namespace Pandora.Domain.Interfaces;

public interface IPermissionRepository
{
    Task<IReadOnlyList<string>> GetAllowedItemIds(long telegramUserId, CancellationToken cancellationToken);
    Task<bool> HasAccess(long telegramUserId, string itemId, CancellationToken cancellationToken);
}
