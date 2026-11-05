using CoreSolution.Shared.Results;
using Identity.Application.Errors;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Interfaces.Repositories;
using Identity.Domain.Interfaces.Services;
using Identity.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Services;

public partial class AdminUserService : IAdminUserService
{
    private readonly IUserRepository _users;
    private readonly ILogger<AdminUserService> _logger;

    public AdminUserService(IUserRepository users, ILogger<AdminUserService> logger)
    {
        _users = users;
        _logger = logger;
    }

    public async Task<IReadOnlyList<EmployeeInfo>> GetUsers(CancellationToken cancellationToken = default)
    {
        return await _users.GetEmployees(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetRoles(CancellationToken cancellationToken = default)
    {
        var roles = await _users.GetAllRoles(cancellationToken);

        return roles.Select(x => x.Name).ToArray();
    }

    public async Task<Result> AssignRoles(Guid userId, string[] roles, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetById(userId, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Назначение ролей отклонено: пользователь не найден");
            return Result.Failure(nameof(IdentityErrorCode.UserNotFound));
        }

        var knownRoles = await _users.GetAllRoles(cancellationToken);
        var rolesByName = knownRoles.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

        var assignedRoles = new List<Roles>(roles.Length);
        foreach (var roleName in roles.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!rolesByName.TryGetValue(roleName, out var role))
            {
                _logger.LogWarning("Назначение ролей отклонено: роль не найдена");
                return Result.Failure(nameof(IdentityErrorCode.RoleNotFound));
            }

            assignedRoles.Add(role);
        }

        user.UserRoles.Clear();
        foreach (var role in assignedRoles)
        {
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        }
        user.UpdatedAt = DateTime.UtcNow;

        await _users.SaveChanges(cancellationToken);

        _logger.LogInformation("Администратор обновил роли пользователя");

        return Result.Success();
    }

    public async Task<Result> SetStatus(Guid userId, UserStatus status, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetById(userId, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Смена статуса отклонена: пользователь не найден");
            return Result.Failure(nameof(IdentityErrorCode.UserNotFound));
        }

        user.Status = status;
        user.UpdatedAt = DateTime.UtcNow;

        await _users.SaveChanges(cancellationToken);

        LogStatusChanged(_logger, status);

        return Result.Success();
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Администратор изменил статус пользователя на {Status}")]
    private static partial void LogStatusChanged(ILogger logger, UserStatus status);
}
