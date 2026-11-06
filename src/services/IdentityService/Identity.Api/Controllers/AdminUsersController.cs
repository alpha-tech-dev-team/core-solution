using Identity.Api.Constants;
using Identity.Api.Extensions.Http;
using Identity.Api.Filters;
using Identity.Application.Contracts.Admin;
using Identity.Domain.Interfaces.Services;
using Identity.Domain.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.Admin)]
[Route(ApiRoutes.AdminGroup)]
public sealed class AdminUsersController(IAdminUserService adminUserService) : ControllerBase
{
    [HttpGet(ApiRoutes.Users)]
    [ValidationFilter<GetUsersRequest>]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersRequest request, CancellationToken cancellationToken)
    {
        var employees = await adminUserService.GetUsers(request.Page, request.PageSize, cancellationToken);

        return Ok(employees);
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
        var result = await adminUserService.CreateUser(request.Adapt<EmployeeData>(), cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut(ApiRoutes.UserById)]
    [ValidationFilter<UserDataRequest>]
    public async Task<IActionResult> UpdateUser(Guid id, UserDataRequest request, CancellationToken cancellationToken)
    {
        var result = await adminUserService.UpdateUser(id, request.Adapt<EmployeeData>(), cancellationToken);

        return result.ToActionResult();
    }

    [HttpDelete(ApiRoutes.UserById)]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        var result = await adminUserService.DeleteUser(id, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut(ApiRoutes.UserRoles)]
    [ValidationFilter<SetUserRolesRequest>]
    public async Task<IActionResult> SetUserRoles(Guid id, SetUserRolesRequest request, CancellationToken cancellationToken)
    {
        var result = await adminUserService.AssignRoles(id, request.Adapt<UserRolesData>(), cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut(ApiRoutes.UserStatus)]
    [ValidationFilter<SetUserStatusRequest>]
    public async Task<IActionResult> SetUserStatus(Guid id, SetUserStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await adminUserService.SetStatus(id, request.Status, cancellationToken);

        return result.ToActionResult();
    }
}
