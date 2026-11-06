using CoreSolution.Shared.Results;
using Identity.Domain.Enums;
using Identity.Domain.Models;

namespace Identity.Domain.Interfaces.Services;

public interface IAdminUserService
{
    Task<IReadOnlyList<EmployeeInfo>> GetUsers(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetRoles(CancellationToken cancellationToken = default);
    Task<Result> CreateUser(EmployeeData data, CancellationToken cancellationToken = default);
    Task<Result> UpdateUser(Guid userId, EmployeeData data, CancellationToken cancellationToken = default);
    Task<Result> AssignRoles(Guid userId, string[] roles, CancellationToken cancellationToken = default);
    Task<Result> SetStatus(Guid userId, UserStatus status, CancellationToken cancellationToken = default);
}
