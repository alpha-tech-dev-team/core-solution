using Atlas.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Atlas.Api.Constants;

internal static class AtlasErrorStatusCodes
{
    private static readonly IReadOnlyDictionary<ErrorCode, int> StatusCodesByError =
        new Dictionary<ErrorCode, int>
        {
            [ErrorCode.ValidationFailed] = StatusCodes.Status400BadRequest,
            [ErrorCode.MalformedRequest] = StatusCodes.Status400BadRequest,
            [ErrorCode.RoomNotFound] = StatusCodes.Status404NotFound,
            [ErrorCode.AccessDenied] = StatusCodes.Status403Forbidden,
            [ErrorCode.Conflict] = StatusCodes.Status409Conflict,
            [ErrorCode.DatabaseUnavailable] = StatusCodes.Status503ServiceUnavailable,
            [ErrorCode.InternalError] = StatusCodes.Status500InternalServerError
        };

    public static int Get(string? errorCode)
    {
        return Enum.TryParse<ErrorCode>(errorCode, out var code)
            ? Get(code)
            : StatusCodes.Status404NotFound;
    }

    public static (int StatusCode, string Code) Map(Exception exception)
    {
        var postgresException = FindPostgresException(exception);

        return postgresException is not null
            ? MapPostgres(postgresException)
            : MapException(exception);
    }

    private static int Get(ErrorCode code)
    {
        return StatusCodesByError.TryGetValue(code, out var statusCode)
            ? statusCode
            : StatusCodes.Status404NotFound;
    }

    private static (int StatusCode, string Code) MapException(Exception exception)
    {
        return exception switch
        {
            BadHttpRequestException badRequest => (badRequest.StatusCode, nameof(ErrorCode.MalformedRequest)),
            DbUpdateException => Fail(ErrorCode.DatabaseUnavailable),
            _ => Fail(ErrorCode.InternalError)
        };
    }

    private static (int StatusCode, string Code) MapPostgres(PostgresException exception)
    {
        return exception.SqlState switch
        {
            PostgresErrorCodes.UniqueViolation => Fail(ErrorCode.Conflict),
            PostgresErrorCodes.ForeignKeyViolation => Fail(ErrorCode.Conflict),
            _ => Fail(exception.IsTransient ? ErrorCode.DatabaseUnavailable : ErrorCode.InternalError)
        };
    }

    private static (int StatusCode, string Code) Fail(ErrorCode code)
    {
        return (Get(code), code.ToString());
    }

    private static PostgresException? FindPostgresException(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException postgresException)
            {
                return postgresException;
            }
        }

        return null;
    }
}
