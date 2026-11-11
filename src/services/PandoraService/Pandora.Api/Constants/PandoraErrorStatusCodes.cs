using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pandora.Domain.Enums;

namespace Pandora.Api.Constants;

internal static class PandoraErrorStatusCodes
{
    private static readonly IReadOnlyDictionary<ErrorCode, int> StatusCodesByError =
        new Dictionary<ErrorCode, int>
        {
            [ErrorCode.InvalidSecretData] = StatusCodes.Status400BadRequest,
            [ErrorCode.MalformedRequest] = StatusCodes.Status400BadRequest,
            [ErrorCode.SecretNotFound] = StatusCodes.Status404NotFound,
            [ErrorCode.VaultwardenUnavailable] = StatusCodes.Status503ServiceUnavailable,
            [ErrorCode.DatabaseUnavailable] = StatusCodes.Status503ServiceUnavailable,
            [ErrorCode.InternalError] = StatusCodes.Status500InternalServerError
        };

    public static int Get(string? errorCode)
    {
        return Enum.TryParse<ErrorCode>(errorCode, out var code)
            ? Get(code)
            : StatusCodes.Status500InternalServerError;
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
            : StatusCodes.Status500InternalServerError;
    }

    private static (int StatusCode, string Code) MapException(Exception exception)
    {
        return exception switch
        {
            BadHttpRequestException badRequest => (badRequest.StatusCode, nameof(ErrorCode.MalformedRequest)),
            HttpRequestException => Fail(ErrorCode.VaultwardenUnavailable),
            TaskCanceledException => Fail(ErrorCode.VaultwardenUnavailable),
            DbUpdateException => Fail(ErrorCode.DatabaseUnavailable),
            _ => Fail(ErrorCode.InternalError)
        };
    }

    private static (int StatusCode, string Code) MapPostgres(PostgresException exception)
    {
        return Fail(exception.IsTransient ? ErrorCode.DatabaseUnavailable : ErrorCode.InternalError);
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
