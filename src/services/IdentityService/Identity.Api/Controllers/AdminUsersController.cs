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
[Authorize(Policy = nameof(RoleType.PandoraAdmin))]
[Route(ApiRoutes.AdminGroup)]
public sealed class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;
    private readonly ICurrentUserService _currentUser;

    public AdminUsersController(IAdminUserService adminUserService, ICurrentUserService currentUser)
    {
        _adminUserService = adminUserService;
        _currentUser = currentUser;
    }

    [HttpGet(ApiRoutes.Users)]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersRequest request, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.GetUsers(request.Page, request.PageSize, request.IncludeArchived, cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet(ApiRoutes.Roles)]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var result = await _adminUserService.GetRoles(cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost(ApiRoutes.Users)]
    public async Task<IActionResult> CreateUser(UserDataRequest request, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.CreateUser(request.Adapt<EmployeeData>(), _currentUser.UserId, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut(ApiRoutes.UserById)]
    public async Task<IActionResult> UpdateUser(Guid id, UserDataRequest request, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.UpdateUser(id, request.Adapt<EmployeeData>(), _currentUser.UserId, cancellationToken);

        return result.ToActionResult();
    }

    [HttpDelete(ApiRoutes.UserById)]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.DeleteUser(id, _currentUser.UserId, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut(ApiRoutes.UserRoles)]
    public async Task<IActionResult> SetUserRoles(Guid id, SetUserRolesRequest request, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.AssignRoles(id, request.Adapt<UserRolesData>(), _currentUser.UserId, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut(ApiRoutes.UserStatus)]
    public async Task<IActionResult> SetUserStatus(Guid id, SetUserStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.SetStatus(id, request.Status, _currentUser.UserId, cancellationToken);

        return result.ToActionResult();
    }
}
