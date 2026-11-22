using Atlas.Api.Constants;
using Atlas.Domain.Enums;
using CoreSolution.Shared.Results;

namespace Atlas.Api.Extensions.Http;

public static class ResultHttpExtensions
{
    public static IResult ToHttpResult(this Result result)
    {
        return result.IsSuccess
            ? Results.Ok()
            : ToProblem(result.ErrorCode);
    }

    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : ToProblem(result.ErrorCode);
    }

    private static IResult ToProblem(string? errorCode)
    {
        var statusCode = AtlasErrorStatusCodes.Get(errorCode);

        return Results.Problem(
            statusCode: statusCode,
            extensions: new Dictionary<string, object?>
            {
                [ProblemFields.Code] = errorCode ?? nameof(ErrorCode.InternalError)
            });
    }
}
