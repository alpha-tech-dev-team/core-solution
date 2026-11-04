using Identity.Api.Constants;
using Identity.Api.Extensions;
using Identity.Api.Filters;
using Identity.Application.Contracts;
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

        return endpoints;
    }

    private static async Task<IResult> Login(
        [FromServices] IAdminAuthService authService,
        AdminLoginRequest request,
        HttpResponse response,
        CancellationToken cancellationToken = default)
    {
        var result = await authService.Login(request.Username, request.Password, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ToHttpResult();
        }

        return RespondWithTokens(result.Value!, response);
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

        if (!result.IsSuccess)
        {
            response.ClearAuthCookies();
            return result.ToHttpResult();
        }

        return RespondWithTokens(result.Value!, response);
    }

    private static IResult Logout(HttpResponse response)
    {
        response.ClearAuthCookies();
        return Results.Ok();
    }

    private static IResult RespondWithTokens(AuthTokens tokens, HttpResponse response)
    {
        response.SetAccessTokenCookie(tokens.AccessToken, tokens.AccessTokenExpiresAt);
        response.SetRefreshTokenCookie(tokens.RefreshToken, tokens.RefreshTokenExpiresAt);
        
        return Results.Ok(new SessionResponse(tokens.AccessTokenExpiresAt));
    }
}
