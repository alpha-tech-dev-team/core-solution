using CoreSolution.Shared.Results;
using Identity.App.Errors;
using Identity.Core.Interfaces.Repositories;
using Identity.Core.Interfaces.Security;
using Identity.Core.Interfaces.Services;
using Identity.Core.Models;
using Microsoft.Extensions.Logging;

namespace Identity.App.Services;

public class AdminAuthService : IAdminAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _tokens;
    private readonly ILogger<AdminAuthService> _logger;

    public AdminAuthService(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtTokenService tokens,
        ILogger<AdminAuthService> logger)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokens = tokens;
        _logger = logger;
    }

    public async Task<Result<AuthTokens>> Login(string username, string password, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByAdminUsername(username, cancellationToken);
        var passwordMatches = _passwordHasher.VerifyPassword(password, user?.AdminPasswordHash);

        if (user is null || !passwordMatches)
        {
            _logger.LogWarning("Вход администратора отклонен: неверный логин или пароль");
            return Result<AuthTokens>.Failure(nameof(IdentityErrorCode.InvalidCredentials));
        }

        _logger.LogInformation("Администратор вошел в систему");

        return Result<AuthTokens>.Success(_tokens.IssueTokens(user));
    }

    public async Task<Result<AuthTokens>> Refresh(string refreshToken, CancellationToken cancellationToken = default)
    {
        var userId = await _tokens.GetUserIdFromRefreshToken(refreshToken);
        var user = userId is null
            ? null
            : await _users.GetById(userId.Value, cancellationToken);

        if (user?.AdminUsername is null || user.AdminPasswordHash is null)
        {
            _logger.LogWarning("Обновление токена отклонено: refresh-токен недействителен");
            return Result<AuthTokens>.Failure(nameof(IdentityErrorCode.InvalidRefreshToken));
        }

        return Result<AuthTokens>.Success(_tokens.IssueTokens(user));
    }
}
