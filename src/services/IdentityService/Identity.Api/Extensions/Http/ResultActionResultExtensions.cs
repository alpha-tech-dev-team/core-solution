using CoreSolution.Shared.Results;
using Identity.Api.Constants;
using Identity.Api.Models;
using Identity.Application.Contracts.Admin;
using Identity.Application.Errors;
using Identity.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Extensions.Http;

public static class ResultActionResultExtensions
{
    public static IActionResult ToActionResult(this Result result)
    {
        return result.IsSuccess
            ? new OkResult()
            : ToErrorResult(result.ErrorCode);
    }

    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? new OkObjectResult(result.Value)
            : ToErrorResult(result.ErrorCode);
    }

    public static IActionResult ToSessionActionResult(
        this Result<AuthTokens> result,
        HttpResponse response,
        bool clearCookiesOnFailure)
    {
        if (result.IsSuccess && result.Value is { } tokens)
        {
            response.SetAccessTokenCookie(tokens.AccessToken, tokens.AccessTokenExpiresAt);
            response.SetRefreshTokenCookie(tokens.RefreshToken, tokens.RefreshTokenExpiresAt);
            return new OkObjectResult(new SessionResponse(tokens.AccessTokenExpiresAt));
        }

        if (clearCookiesOnFailure)
        {
            response.ClearAuthCookies();
        }

        return result.ToActionResult();
    }

    private static IActionResult ToErrorResult(string? errorCode)
    {
        var code = errorCode ?? nameof(IdentityErrorCode.InternalError);

        return new ObjectResult(new ErrorResponse(code))
        {
            StatusCode = IdentityErrorStatusCodes.Get(code)
        };
    }
}
