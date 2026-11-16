using Pandora.Domain.Models;

namespace Pandora.Domain.Interfaces;

public interface IPermissionRepository
{
    Task<IReadOnlyList<string>> GetAllowedItemIds(long telegramUserId, CancellationToken cancellationToken);
    Task<bool> HasAccess(long telegramUserId, string itemId, CancellationToken cancellationToken);
    Task<EmployeeSecretPage> GetPage(long telegramUserId, SecretPageQuery query, CancellationToken cancellationToken);
    Task<SecretAccessGrantResult> Grant(
        long telegramUserId,
        string itemId,
        Guid adminUserId,
        CancellationToken cancellationToken);
    Task<SecretAccessRevokeResult> Revoke(long telegramUserId, string itemId, CancellationToken cancellationToken);
    Task SaveChanges(CancellationToken cancellationToken);
}
