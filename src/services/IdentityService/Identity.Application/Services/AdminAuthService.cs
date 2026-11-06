using CoreSolution.Shared.Results;
using Identity.Application.Errors;
using Identity.Domain.Enums;
using Identity.Domain.Interfaces.Repositories;
using Identity.Domain.Interfaces.Security;
using Identity.Domain.Interfaces.Services;
using Identity.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Services;

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

        if (user.Status == UserStatus.Blocked)
        {
            _logger.LogWarning("Вход администратора отклонен: пользователь заблокирован");
            return Result<AuthTokens>.Failure(nameof(IdentityErrorCode.AccessDenied));
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

        if (user.Status == UserStatus.Blocked)
        {
            _logger.LogWarning("Обновление токена отклонено: пользователь заблокирован");
            return Result<AuthTokens>.Failure(nameof(IdentityErrorCode.InvalidRefreshToken));
        }

        return Result<AuthTokens>.Success(_tokens.IssueTokens(user));
    }
}
