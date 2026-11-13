using Identity.Domain.Entities;
using Identity.Domain.Models;

namespace Identity.Domain.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetById(Guid id, bool includeRoles = false, CancellationToken cancellationToken = default);
    Task<User?> GetByPhone(string phone, CancellationToken cancellationToken = default);
    Task<User?> GetByPhoneIncludingArchived(string phone, CancellationToken cancellationToken = default);
    Task<User?> GetByTelegramUserId(long telegramUserId, CancellationToken cancellationToken = default);
    Task<User?> GetByAdminUsername(string username, CancellationToken cancellationToken = default);
    Task<EmployeesPage> GetEmployees(int page, int pageSize, bool includeArchived = false, CancellationToken cancellationToken = default);
    Task<List<Roles>> GetAllRoles(CancellationToken cancellationToken = default);
}
