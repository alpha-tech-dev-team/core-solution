using CoreSolution.Shared.Results;
using Pandora.Api.Constants;

namespace Pandora.Api.Extensions;

public static class ResultHttpExtensions
{
    public static IResult ToHttpResult(this Result result)
    {
        return result.IsSuccess
            ? Results.Ok()
            : Results.StatusCode(PandoraErrorStatusCodes.Get(result.ErrorCode));
    }

    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.StatusCode(PandoraErrorStatusCodes.Get(result.ErrorCode));
    }
}
