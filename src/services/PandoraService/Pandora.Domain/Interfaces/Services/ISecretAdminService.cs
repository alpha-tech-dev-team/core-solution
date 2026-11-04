using CoreSolution.Shared.Results;
using Pandora.Domain.Models;

namespace Pandora.Domain.Interfaces.Services;

public interface ISecretAdminService
{
    Task<Result<IReadOnlyList<SecretListItem>>> GetItems(CancellationToken cancellationToken);
    Task<Result<SecretListItem>> Create(Guid adminUserId, string name, string username, string password, CancellationToken cancellationToken);
    Task<Result<SecretListItem>> Update(Guid adminUserId, string itemId, string name, string username, string? password, CancellationToken cancellationToken);
    Task<Result> Delete(Guid adminUserId, string itemId, CancellationToken cancellationToken);
}
