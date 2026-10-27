using CoreSolution.Shared.Results;
using Identity.Api.Constants;

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
}
