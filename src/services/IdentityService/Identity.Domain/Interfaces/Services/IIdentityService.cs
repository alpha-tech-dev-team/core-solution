using CoreSolution.Shared.Results;
using Identity.Domain.Models;

namespace Identity.Domain.Interfaces.Services;

public interface IIdentityService
{
    Task<Result> ActivateUser(long telegramUserId, string phone, CancellationToken cancellationToken = default);
    Task<Result> CheckUser(long telegramUserId, string access, CancellationToken cancellationToken = default);
    Task<Result<EmployeeProfile>> GetEmployee(long telegramUserId, CancellationToken cancellationToken = default);
}
