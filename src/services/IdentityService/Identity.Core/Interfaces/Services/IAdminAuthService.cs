using CoreSolution.Shared.Results;
using Identity.Core.Models;

namespace Identity.Core.Interfaces.Services;

public interface IAdminAuthService
{
    Task<Result<AuthTokens>> Login(string username, string password, CancellationToken cancellationToken = default);
    Task<Result<AuthTokens>> Refresh(string refreshToken, CancellationToken cancellationToken = default);
}
