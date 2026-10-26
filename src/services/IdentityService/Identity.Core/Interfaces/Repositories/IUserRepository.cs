using Identity.Core.Entities;

namespace Identity.Core.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByPhone(string phone, CancellationToken cancellationToken = default);
    Task<User?> GetByTelegramUserId(long telegramUserId, CancellationToken cancellationToken = default);
    Task<User?> GetByAdminUsername(string username, CancellationToken cancellationToken = default);
    Task SaveChanges(CancellationToken cancellationToken = default);
}
