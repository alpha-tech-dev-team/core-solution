using Identity.Api.Constants;
using Identity.Api.Extensions.Http;
using Identity.Api.Filters;
using Identity.Application.Contracts.Admin;
using Identity.Domain.Enums;
using Identity.Domain.Interfaces.Services;
using Identity.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.Admin)]
[Route(ApiRoutes.AdminGroup)]
public sealed class AdminUsersController(IAdminUserService adminUserService) : ControllerBase
{
    [HttpGet(ApiRoutes.Users)]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var users = await adminUserService.GetUsers(cancellationToken);

        return Ok(users);
    }

    [HttpGet(ApiRoutes.Roles)]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var roles = await adminUserService.GetRoles(cancellationToken);

        return Ok(roles);
    }

    [HttpPost(ApiRoutes.Users)]
    [ValidationFilter<UserDataRequest>]
    public async Task<IActionResult> CreateUser(UserDataRequest request, CancellationToken cancellationToken)
    {
        var result = await adminUserService.CreateUser(ToEmployeeData(request), cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut(ApiRoutes.UserById)]
    [ValidationFilter<UserDataRequest>]
    public async Task<IActionResult> UpdateUser(Guid id, UserDataRequest request, CancellationToken cancellationToken)
    {
        var result = await adminUserService.UpdateUser(id, ToEmployeeData(request), cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut(ApiRoutes.UserRoles)]
    [ValidationFilter<SetUserRolesRequest>]
    public async Task<IActionResult> SetUserRoles(Guid id, SetUserRolesRequest request, CancellationToken cancellationToken)
    {
        var result = await adminUserService.AssignRoles(id, request.Roles, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut(ApiRoutes.UserStatus)]
    [ValidationFilter<SetUserStatusRequest>]
    public async Task<IActionResult> SetUserStatus(Guid id, SetUserStatusRequest request, CancellationToken cancellationToken)
    {
        var status = Enum.Parse<UserStatus>(request.Status, ignoreCase: true);
        var result = await adminUserService.SetStatus(id, status, cancellationToken);

        return result.ToActionResult();
    }

    private static EmployeeData ToEmployeeData(UserDataRequest request)
    {
        return new EmployeeData(request.Name, request.Surname, request.Patronymic, request.Position, request.Phone);
    }
}
