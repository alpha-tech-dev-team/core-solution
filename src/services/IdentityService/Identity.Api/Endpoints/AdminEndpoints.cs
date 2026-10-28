using Identity.Api.Constants;
using Identity.Api.Extensions;
using Identity.Api.Filters;
using Identity.App.Contracts;
using Identity.Core.Interfaces.Services;
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
            .AddEndpointFilter<ValidationFilter<RefreshTokenRequest>>()
            .RequireRateLimiting(RateLimitingPolicies.Auth);

        return endpoints;
    }

    private static async Task<IResult> Login(
        [FromServices] IAdminAuthService authService,
        AdminLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await authService.Login(request.Username, request.Password, cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> RefreshToken(
        [FromServices] IAdminAuthService authService,
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await authService.Refresh(request.RefreshToken, cancellationToken);

        return result.ToHttpResult();
    }
}
