using Identity.Core.Entities;
using Identity.Core.Models;

namespace Identity.Core.Interfaces.Security;

public interface IJwtTokenService
{
    AuthTokens IssueTokens(User user);
    Task<Guid?> GetUserIdFromRefreshToken(string refreshToken);
}
