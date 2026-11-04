using Identity.Api.Constants;
using Identity.Api.Extensions;
using Identity.Api.Filters;
using Identity.Application.Contracts;
using Identity.Domain.Interfaces.Services;
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

        return endpoints;
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
