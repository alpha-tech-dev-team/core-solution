using CoreSolution.Shared.Results;

namespace Identity.Core.Interfaces.Services;

public interface IIdentityService
{
    Task<Result> ActivateUser(long telegramUserId, string phone, CancellationToken cancellationToken = default);
    Task<Result> CheckUser(long telegramUserId, string access, CancellationToken cancellationToken = default);
}
