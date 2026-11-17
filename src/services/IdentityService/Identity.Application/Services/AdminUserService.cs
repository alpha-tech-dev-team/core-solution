using CoreSolution.Shared.Results;
using Identity.Application.Errors;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Interfaces.Repositories;
using Identity.Domain.Interfaces.Security;
using Identity.Domain.Interfaces.Services;
using Identity.Domain.Models;
using Mapster;
using Microsoft.Extensions.Logging;
using RoleType = CoreSolution.Shared.Enums.UserRole;
using UserRoleEntity = Identity.Domain.Entities.UserRole;

namespace Identity.Application.Services;

public class AdminUserService : IAdminUserService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AdminUserService> _logger;

    public AdminUserService(IUserRepository users, IPasswordHasher passwordHasher, ILogger<AdminUserService> logger)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<Result<EmployeesPage>> GetUsers(int page, int pageSize, bool includeArchived, CancellationToken cancellationToken = default)
    {
        var employees = await _users.GetEmployees(page, pageSize, includeArchived, cancellationToken);

        return Result<EmployeesPage>.Success(employees);
    }

    public async Task<Result<IReadOnlyList<string>>> GetRoles(CancellationToken cancellationToken = default)
    {
        var roles = await _users.GetAllRoles(cancellationToken);

        return Result<IReadOnlyList<string>>.Success(roles.Select(x => x.Name).ToArray());
    }

    public async Task<Result> CreateUser(EmployeeData data, Guid actorId, CancellationToken cancellationToken = default)
    {
        var existing = await _users.GetByPhone(data.Phone, includeArchived: true, cancellationToken);
        if (existing is not null)
        {
            _logger.LogWarning("Добавление сотрудника отклонено: телефон уже занят");
            return Result.Failure(nameof(IdentityErrorCode.PhoneAlreadyExists));
        }

        var user = new User(data)
        {
            UpdatedById = actorId
        };

        _users.Add(user);
        await _users.SaveChanges(cancellationToken);

        _logger.LogInformation("Администратор добавил сотрудника");

        return Result.Success();
    }

    public async Task<Result> UpdateUser(Guid userId, EmployeeData data, Guid actorId, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetById(userId, cancellationToken: cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Изменение сотрудника отклонено: пользователь не найден");
            return Result.Failure(nameof(IdentityErrorCode.UserNotFound));
        }

        if (!string.Equals(user.Phone, data.Phone, StringComparison.Ordinal))
        {
            var existing = await _users.GetByPhone(data.Phone, includeArchived: true, cancellationToken);
            if (existing is not null && existing.Id != userId)
            {
                _logger.LogWarning("Изменение сотрудника отклонено: телефон уже занят");
                return Result.Failure(nameof(IdentityErrorCode.PhoneAlreadyExists));
            }
        }

        data.Adapt(user);
        user.UpdatedById = actorId;

        await _users.SaveChanges(cancellationToken);

        _logger.LogInformation("Администратор изменил данные сотрудника");

        return Result.Success();
    }

    public async Task<Result> AssignRoles(Guid userId, UserRolesData data, Guid actorId, CancellationToken cancellationToken = default)
    {
        if (userId == actorId && !data.Roles.Contains(RoleType.PandoraAdmin))
        {
            _logger.LogWarning("Назначение ролей отклонено: администратор пытается снять с себя роль PandoraAdmin");
            return Result.Failure(nameof(IdentityErrorCode.SelfActionForbidden));
        }

        var user = await _users.GetById(userId, includeRoles: true, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Назначение ролей отклонено: пользователь не найден");
            return Result.Failure(nameof(IdentityErrorCode.UserNotFound));
        }

        if (!string.IsNullOrWhiteSpace(data.AdminLogin)
            && !string.IsNullOrWhiteSpace(data.AdminPassword))
        {
            user.AdminUsername = data.AdminLogin;
            user.AdminPasswordHash = _passwordHasher.HashPassword(data.AdminPassword);
        }

        var knownRoles = await _users.GetAllRoles(cancellationToken);
        var rolesByName = knownRoles.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

        var assignedRoles = new List<Roles>(data.Roles.Length);
        foreach (var roleName in data.Roles.Distinct())
        {
            if (!rolesByName.TryGetValue(roleName.ToString(), out var role))
            {
                _logger.LogWarning("Назначение ролей отклонено: роль не найдена");
                return Result.Failure(nameof(IdentityErrorCode.RoleNotFound));
            }

            assignedRoles.Add(role);
        }

        user.UserRoles.Clear();
        foreach (var role in assignedRoles)
        {
            user.UserRoles.Add(new UserRoleEntity { UserId = user.Id, RoleId = role.Id });
        }

        user.UpdatedById = actorId;

        await _users.SaveChanges(cancellationToken);

        _logger.LogInformation("Администратор обновил роли пользователя");

        return Result.Success();
    }

    public async Task<Result> DeleteUser(Guid userId, Guid actorId, CancellationToken cancellationToken = default)
    {
        if (userId == actorId)
        {
            _logger.LogWarning("Удаление сотрудника отклонено: администратор пытается удалить самого себя");
            return Result.Failure(nameof(IdentityErrorCode.SelfActionForbidden));
        }

        var user = await _users.GetById(userId, cancellationToken: cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Удаление сотрудника отклонено: пользователь не найден");
            return Result.Failure(nameof(IdentityErrorCode.UserNotFound));
        }

        user.Status = UserStatus.Archived;
        user.UpdatedById = actorId;

        await _users.SaveChanges(cancellationToken);

        _logger.LogInformation("Администратор перенес сотрудника в архив");

        return Result.Success();
    }

    public async Task<Result> SetStatus(Guid userId, UserStatus status, Guid actorId, CancellationToken cancellationToken = default)
    {
        if (userId == actorId && status is UserStatus.Blocked or UserStatus.Archived)
        {
            _logger.LogWarning("Смена статуса отклонена: администратор пытается заблокировать или архивировать самого себя");
            return Result.Failure(nameof(IdentityErrorCode.SelfActionForbidden));
        }

        var user = await _users.GetById(userId, cancellationToken: cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Смена статуса отклонена: пользователь не найден");
            return Result.Failure(nameof(IdentityErrorCode.UserNotFound));
        }

        user.Status = status == UserStatus.Active && user.TelegramUserId is null
            ? UserStatus.Pending
            : status;
        user.UpdatedById = actorId;

        await _users.SaveChanges(cancellationToken);

        _logger.LogInformation("Администратор изменил статус пользователя {UserId} на {Status}", user.Id, user.Status);

        return Result.Success();
    }
}
