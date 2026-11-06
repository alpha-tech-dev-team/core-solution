using Identity.Api.Constants;
using Identity.Api.Extensions.Http;
using Identity.Api.Filters;
using Identity.Application.Contracts.Admin;
using Identity.Domain.Enums;
using Identity.Domain.Interfaces.Services;
using Identity.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(ApiRoutes.AdminGroup);

        group.MapPost(ApiRoutes.Login, Login)
            .AddEndpointFilter<ValidationFilter<AdminLoginRequest>>()
            .RequireRateLimiting(RateLimitingPolicies.Auth);

        group.MapPost(ApiRoutes.RefreshToken, RefreshToken)
            .RequireRateLimiting(RateLimitingPolicies.Auth);

        group.MapPost(ApiRoutes.Logout, Logout);

        group.MapGet(ApiRoutes.Users, GetUsers)
            .RequireAuthorization(AuthorizationPolicies.Admin);

        group.MapPost(ApiRoutes.Users, CreateUser)
            .AddEndpointFilter<ValidationFilter<UserDataRequest>>()
            .RequireAuthorization(AuthorizationPolicies.Admin);

        group.MapPut(ApiRoutes.UserById, UpdateUser)
            .AddEndpointFilter<ValidationFilter<UserDataRequest>>()
            .RequireAuthorization(AuthorizationPolicies.Admin);

        group.MapGet(ApiRoutes.Roles, GetRoles)
            .RequireAuthorization(AuthorizationPolicies.Admin);

        group.MapPut(ApiRoutes.UserRoles, SetUserRoles)
            .AddEndpointFilter<ValidationFilter<SetUserRolesRequest>>()
            .RequireAuthorization(AuthorizationPolicies.Admin);

        group.MapPut(ApiRoutes.UserStatus, SetUserStatus)
            .AddEndpointFilter<ValidationFilter<SetUserStatusRequest>>()
            .RequireAuthorization(AuthorizationPolicies.Admin);

        return endpoints;
    }

    private static async Task<IResult> GetUsers(
        [FromServices] IAdminUserService adminUserService,
        CancellationToken cancellationToken = default)
    {
        var users = await adminUserService.GetUsers(cancellationToken);

        return Results.Ok(users);
    }

    private static async Task<IResult> GetRoles(
        [FromServices] IAdminUserService adminUserService,
        CancellationToken cancellationToken = default)
    {
        var roles = await adminUserService.GetRoles(cancellationToken);

        return Results.Ok(roles);
    }

    private static async Task<IResult> CreateUser(
        [FromServices] IAdminUserService adminUserService,
        UserDataRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await adminUserService.CreateUser(ToEmployeeData(request), cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateUser(
        [FromServices] IAdminUserService adminUserService,
        Guid id,
        UserDataRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await adminUserService.UpdateUser(id, ToEmployeeData(request), cancellationToken);

        return result.ToHttpResult();
    }

    private static EmployeeData ToEmployeeData(UserDataRequest request)
    {
        return new EmployeeData(request.Name, request.Surname, request.Patronymic, request.Position, request.Phone);
    }

    private static async Task<IResult> SetUserRoles(
        [FromServices] IAdminUserService adminUserService,
        Guid id,
        SetUserRolesRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await adminUserService.AssignRoles(id, request.Roles, cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> SetUserStatus(
        [FromServices] IAdminUserService adminUserService,
        Guid id,
        SetUserStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var status = Enum.Parse<UserStatus>(request.Status, ignoreCase: true);
        var result = await adminUserService.SetStatus(id, status, cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> Login(
        [FromServices] IAdminAuthService authService,
        AdminLoginRequest request,
        HttpResponse response,
        CancellationToken cancellationToken = default)
    {
        var result = await authService.Login(request.Username, request.Password, cancellationToken);

        return result.ToSessionResult(response, clearCookiesOnFailure: false);
    }

    private static async Task<IResult> RefreshToken(
        [FromServices] IAdminAuthService authService,
        HttpRequest request,
        HttpResponse response,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = request.GetRefreshTokenCookie();

        if (string.IsNullOrEmpty(refreshToken))
        {
            return Results.Unauthorized();
        }

        var result = await authService.Refresh(refreshToken, cancellationToken);

        return result.ToSessionResult(response, clearCookiesOnFailure: true);
    }

    private static IResult Logout(HttpResponse response)
    {
        response.ClearAuthCookies();
        return Results.Ok();
    }
}
