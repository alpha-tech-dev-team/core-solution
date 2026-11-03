using Identity.Domain.Entities;
using Identity.Domain.Models;

namespace Identity.Domain.Interfaces.Security;

public interface IJwtTokenService
{
    AuthTokens IssueTokens(User user);
    Task<Guid?> GetUserIdFromRefreshToken(string refreshToken);
}
