using CoreSolution.Shared.Results;
using Identity.Api.Constants;
using Identity.Application.Contracts;
using Identity.Domain.Models;

namespace Identity.Api.Extensions;

public static class ResultHttpExtensions
{
    public static IResult ToHttpResult(this Result result)
    {
        return result.IsSuccess
            ? Results.Ok()
            : Results.StatusCode(IdentityErrorStatusCodes.Get(result.ErrorCode));
    }

    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.StatusCode(IdentityErrorStatusCodes.Get(result.ErrorCode));
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
}
