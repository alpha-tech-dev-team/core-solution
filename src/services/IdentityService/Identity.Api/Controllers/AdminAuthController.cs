using CoreSolution.Shared.Results;
using Identity.Api.Constants;
using Identity.Api.Extensions.Http;
using Identity.Application.Contracts.Admin;
using Identity.Application.Errors;
using Identity.Domain.Interfaces.Services;
using Identity.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Identity.Api.Controllers;

[ApiController]
[Route(ApiRoutes.AdminGroup)]
public sealed class AdminAuthController(IAdminAuthService authService) : ControllerBase
{
    [HttpPost(ApiRoutes.Login)]
    [EnableRateLimiting(RateLimitingPolicies.Auth)]
    public async Task<IActionResult> Login(AdminLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.Login(request.Username, request.Password, cancellationToken);

        return result.ToSessionActionResult(Response, clearCookiesOnFailure: false);
    }

    [HttpPost(ApiRoutes.RefreshToken)]
    [EnableRateLimiting(RateLimitingPolicies.Auth)]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = Request.GetRefreshTokenCookie();

        var result = string.IsNullOrEmpty(refreshToken)
            ? Result<AuthTokens>.Failure(nameof(IdentityErrorCode.InvalidRefreshToken))
            : await authService.Refresh(refreshToken, cancellationToken);

        return result.ToSessionActionResult(Response, clearCookiesOnFailure: true);
    }

    [HttpPost(ApiRoutes.Logout)]
    public IActionResult Logout()
    {
        Response.ClearAuthCookies();

        return Ok();
    }
}
