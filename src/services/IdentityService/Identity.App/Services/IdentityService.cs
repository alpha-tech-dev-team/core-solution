using CoreSolution.Shared.Results;
using Identity.App.Errors;
using Identity.Core.Enums;
using Identity.Core.Interfaces.Repositories;
using Identity.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Identity.App.Services;

public class IdentityService : IIdentityService
{
    private readonly IUserRepository _users;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(IUserRepository users, ILogger<IdentityService> logger)
    {
        _users = users;
        _logger = logger;
    }

    public async Task<Result> ActivateUser(long telegramUserId, string phone, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByPhone(phone, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Активация пропущена: пользователь не найден но номеру");
            return Result.Failure(nameof(IdentityErrorCode.UserNotFound));
        }

        if (user.TelegramUserId is not null && user.TelegramUserId != telegramUserId)
        {
            _logger.LogWarning("Активания отклонена: пользователь уже активирован для другого номера");
            return Result.Failure(nameof(IdentityErrorCode.UserAlreadyActivated));
        }

        if (user.Status == UserStatus.Active)
        {
            _logger.LogInformation("Активация пропущена: пользователь уже активирован");
            return user.TelegramUserId == telegramUserId
                ? Result.Success()
                : Result.Failure(nameof(IdentityErrorCode.UserAlreadyActivated));
        }

        user.TelegramUserId = telegramUserId;
        user.Status = UserStatus.Active;
        user.UpdatedAt = DateTime.UtcNow;

        await _users.SaveChanges(cancellationToken);

        _logger.LogInformation("Пользователь активировал аккаунт");

        return Result.Success();
    }

    public async Task<Result> CheckUser(long telegramUserId, string access, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByTelegramUserId(telegramUserId, cancellationToken);

        var hasAccess = user is not null
            && user.Status == UserStatus.Active
            && user.UserRoles.Any(x => string.Equals(x.Role.Name, access, StringComparison.OrdinalIgnoreCase));

        if (!hasAccess)
        {
            _logger.LogInformation("Доступ запрещен пользователю");
            return Result.Failure(nameof(IdentityErrorCode.AccessDenied));
        }

        return Result.Success();
    }
}
