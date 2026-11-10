using Identity.Api.Constants;
using Identity.Api.Extensions.Http;
using Identity.Api.Models;
using Identity.Api.Interfaces;
using Identity.Application.Contracts.Admin;
using Identity.Application.Errors;
using Identity.Domain.Interfaces.Services;
using Identity.Domain.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoleType = CoreSolution.Shared.Enums.UserRole;

namespace Identity.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.PandoraAdmin)]
[Route(ApiRoutes.AdminGroup)]
public sealed class AdminUsersController(IAdminUserService adminUserService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet(ApiRoutes.Users)]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersRequest request, CancellationToken cancellationToken)
    {
        var result = await adminUserService.GetUsers(request.Page, request.PageSize, cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet(ApiRoutes.Roles)]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var result = await adminUserService.GetRoles(cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost(ApiRoutes.Users)]
    public async Task<IActionResult> CreateUser(UserDataRequest request, CancellationToken cancellationToken)
    {
        var result = await adminUserService.CreateUser(request.Adapt<EmployeeData>(), currentUser.UserId, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut(ApiRoutes.UserById)]
    public async Task<IActionResult> UpdateUser(Guid id, UserDataRequest request, CancellationToken cancellationToken)
    {
        var result = await adminUserService.UpdateUser(id, request.Adapt<EmployeeData>(), currentUser.UserId, cancellationToken);

        return result.ToActionResult();
    }

    [HttpDelete(ApiRoutes.UserById)]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        var result = await adminUserService.DeleteUser(id, cancellationToken);

        return result.IsSuccess && currentUser.IsCurrentUser(id)
            ? InvalidateCurrentSession()
            : result.ToActionResult();
    }

    [HttpPut(ApiRoutes.UserRoles)]
    public async Task<IActionResult> SetUserRoles(Guid id, SetUserRolesRequest request, CancellationToken cancellationToken)
    {
        var result = await adminUserService.AssignRoles(id, request.Adapt<UserRolesData>(), currentUser.UserId, cancellationToken);

        return result.IsSuccess && currentUser.IsCurrentUser(id) && !request.Roles.Contains(RoleType.PandoraAdmin)
            ? InvalidateCurrentSession()
            : result.ToActionResult();
    }

    [HttpPut(ApiRoutes.UserStatus)]
    public async Task<IActionResult> SetUserStatus(Guid id, SetUserStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await adminUserService.SetStatus(id, request.Status, currentUser.UserId, cancellationToken);

        return result.ToActionResult();
    }

    private IActionResult InvalidateCurrentSession()
    {
        Response.ClearAuthCookies();

        var code = nameof(IdentityErrorCode.AccessDenied);
        return StatusCode(StatusCodes.Status401Unauthorized, new ErrorResponse(code));
    }
}
