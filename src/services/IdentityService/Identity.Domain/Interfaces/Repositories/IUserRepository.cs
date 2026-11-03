using Identity.Domain.Entities;

namespace Identity.Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByPhone(string phone, CancellationToken cancellationToken = default);
    Task<User?> GetByTelegramUserId(long telegramUserId, CancellationToken cancellationToken = default);
    Task<User?> GetByAdminUsername(string username, CancellationToken cancellationToken = default);
    Task<int> SaveChanges(CancellationToken cancellationToken = default);
}
