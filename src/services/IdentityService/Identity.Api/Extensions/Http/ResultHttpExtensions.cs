using CoreSolution.Shared.Results;
using Identity.Api.Constants;
using Identity.Api.Models;
using Identity.Application.Contracts.Admin;
using Identity.Application.Errors;
using Identity.Domain.Models;

namespace Identity.Api.Extensions.Http;

public static class ResultHttpExtensions
{
    public static IResult ToHttpResult(this Result result)
    {
        return result.IsSuccess
            ? Results.Ok()
            : ToErrorResult(result.ErrorCode);
    }

    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : ToErrorResult(result.ErrorCode);
    }

    public static IResult ToSessionResult(
        this Result<AuthTokens> result,
        HttpResponse response,
        bool clearCookiesOnFailure)
    {
        if (result.IsSuccess && result.Value is { } tokens)
        {
            response.SetAccessTokenCookie(tokens.AccessToken, tokens.AccessTokenExpiresAt);
            response.SetRefreshTokenCookie(tokens.RefreshToken, tokens.RefreshTokenExpiresAt);
            return Results.Ok(new SessionResponse(tokens.AccessTokenExpiresAt));
        }

        if (clearCookiesOnFailure)
        {
            response.ClearAuthCookies();
        }

        return result.ToHttpResult();
    }

    private static IResult ToErrorResult(string? errorCode)
    {
        var code = errorCode ?? nameof(IdentityErrorCode.InternalError);

        return Results.Json(new ErrorResponse(code), statusCode: IdentityErrorStatusCodes.Get(code));
    }
}
