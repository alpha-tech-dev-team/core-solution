using CoreSolution.Shared.Results;
using Identity.Application.Errors;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Interfaces.Repositories;
using Identity.Domain.Interfaces.Services;
using Identity.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Services;

public class AdminUserService : IAdminUserService
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

    public async Task<Result> CreateUser(EmployeeData data, CancellationToken cancellationToken = default)
    {
        var existing = await _users.GetByPhone(data.Phone, cancellationToken);
        if (existing is not null)
        {
            _logger.LogWarning("Добавление сотрудника отклонено: телефон уже занят");
            return Result.Failure(nameof(IdentityErrorCode.PhoneAlreadyExists));
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = data.Name,
            Surname = data.Surname,
            Patronymic = data.Patronymic,
            Position = data.Position,
            Phone = data.Phone,
            Status = UserStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        user.UpdatedById = user.Id;

        _users.Add(user);
        await _users.SaveChanges(cancellationToken);

        _logger.LogInformation("Администратор добавил сотрудника");

        return Result.Success();
    }

    public async Task<Result> UpdateUser(Guid userId, EmployeeData data, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetById(userId, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Изменение сотрудника отклонено: пользователь не найден");
            return Result.Failure(nameof(IdentityErrorCode.UserNotFound));
        }

        if (!string.Equals(user.Phone, data.Phone, StringComparison.Ordinal))
        {
            var existing = await _users.GetByPhone(data.Phone, cancellationToken);
            if (existing is not null)
            {
                _logger.LogWarning("Изменение сотрудника отклонено: телефон уже занят");
                return Result.Failure(nameof(IdentityErrorCode.PhoneAlreadyExists));
            }
        }

        user.Name = data.Name;
        user.Surname = data.Surname;
        user.Patronymic = data.Patronymic;
        user.Position = data.Position;
        user.Phone = data.Phone;
        user.UpdatedAt = DateTime.UtcNow;

        await _users.SaveChanges(cancellationToken);

        _logger.LogInformation("Администратор изменил данные сотрудника");

        return Result.Success();
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

        user.Status = status == UserStatus.Active && user.TelegramUserId is null
            ? UserStatus.Pending
            : status;
        user.UpdatedAt = DateTime.UtcNow;

        await _users.SaveChanges(cancellationToken);

        _logger.LogInformation("Администратор изменил статус пользователя {UserId} на {Status}", user.Id, user.Status);

        return Result.Success();
    }
}
