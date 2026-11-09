using Pandora.Domain.Models;

namespace Pandora.Domain.Interfaces;

public interface IPermissionRepository
{
    Task<IReadOnlyList<string>> GetAllowedItemIds(long telegramUserId, CancellationToken cancellationToken);
    Task<bool> HasAccess(long telegramUserId, string itemId, CancellationToken cancellationToken);
    Task<EmployeeSecretPage> GetPage(long telegramUserId, int page, int pageSize, CancellationToken cancellationToken);
    Task<bool> Grant(long telegramUserId, string itemId, CancellationToken cancellationToken);
    Task<bool> Revoke(long telegramUserId, string itemId, CancellationToken cancellationToken);
    Task SaveChanges(CancellationToken cancellationToken);
}
