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

    public async Task<bool> ActivateUser(long telegramUserId, string phone, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByPhone(phone, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Активация пропущена: пользователь не найден но номеру");
            return false;
        }

        if (user.TelegramUserId is not null && user.TelegramUserId != telegramUserId)
        {
            _logger.LogWarning("Активания отклонена: пользователь уже активирован для другого номера");
            return false;
        }

        if (user.Status == UserStatus.Active)
        {
            _logger.LogInformation("Активация пропущена: пользователь уже активирован");
            return user.TelegramUserId == telegramUserId;
        }   

        user.TelegramUserId = telegramUserId;
        user.Status = UserStatus.Active;
        user.UpdatedAt = DateTime.UtcNow;

        await _users.SaveChanges(cancellationToken);

        _logger.LogInformation("Пользователь активировал аккаунт");

        return true;
    }

    public async Task<bool> CheckUser(long telegramUserId, string access, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByTelegramUserId(telegramUserId, cancellationToken);

        var hasAccess = user is not null
            && user.Status == UserStatus.Active
            && user.UserRoles.Any(x => string.Equals(x.Role.Name, access, StringComparison.OrdinalIgnoreCase));

        if (!hasAccess)
        {
            _logger.LogInformation("Доступ запрещен пользователю");
        }

        return hasAccess;
    }
}
