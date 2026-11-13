using CoreSolution.Shared.Enums;
using CoreSolution.Shared.Results;
using Identity.Domain.Enums;
using Identity.Domain.Models;

namespace Identity.Domain.Interfaces.Services;

public interface IAdminUserService
{
    Task<Result<EmployeesPage>> GetUsers(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<string>>> GetRoles(CancellationToken cancellationToken = default);
    Task<Result> CreateUser(EmployeeData data, Guid actorId, CancellationToken cancellationToken = default);
    Task<Result> UpdateUser(Guid userId, EmployeeData data, Guid actorId, CancellationToken cancellationToken = default);
    Task<Result> AssignRoles(Guid userId, UserRolesData data, Guid actorId, CancellationToken cancellationToken = default);
    Task<Result> SetStatus(Guid userId, UserStatus status, Guid actorId, CancellationToken cancellationToken = default);
    Task<Result> DeleteUser(Guid userId, Guid actorId, CancellationToken cancellationToken = default);
}
