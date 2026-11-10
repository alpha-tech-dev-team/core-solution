using CoreSolution.Shared.Results;
using Pandora.Api.Constants;
using Pandora.Api.Models;
using Pandora.Domain.Enums;

namespace Pandora.Api.Extensions;

public static class ResultHttpExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : ToErrorResult(result.ErrorCode);
    }

    private static IResult ToErrorResult(string? errorCode)
    {
        var code = errorCode ?? nameof(ErrorCode.InternalError);

        return Results.Json(new ErrorResponse(code), statusCode: PandoraErrorStatusCodes.Get(code));
    }
}
