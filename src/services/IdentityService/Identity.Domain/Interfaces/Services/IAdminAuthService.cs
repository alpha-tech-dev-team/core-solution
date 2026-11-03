using CoreSolution.Shared.Results;
using Identity.Domain.Models;

namespace Identity.Domain.Interfaces.Services;

public interface IAdminAuthService
{
    Task<Result<AuthTokens>> Login(string username, string password, CancellationToken cancellationToken = default);
    Task<Result<AuthTokens>> Refresh(string refreshToken, CancellationToken cancellationToken = default);
}
