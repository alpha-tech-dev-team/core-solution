using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pandora.Api.Constants;
using Pandora.Domain.Enums;

namespace Pandora.Api.Middleware;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (context.Response.HasStarted)
        {
            return false;
        }

        if (exception is OperationCanceledException canceled && canceled.CancellationToken == context.RequestAborted)
        {
            _logger.LogInformation("Запрос отменён клиентом при {Method} {Path}", context.Request.Method, context.Request.Path);

            return true;
        }

        var (statusCode, code) = Map(exception);

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Необработанное исключение при {Method} {Path}", context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning("Запрос отклонён со статусом {StatusCode} при {Method} {Path}", statusCode, context.Request.Method, context.Request.Path);
        }

        context.Response.StatusCode = statusCode;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = ReasonPhrases.GetReasonPhrase(statusCode),
                Extensions = { [ProblemFields.Code] = code }
            }
        });
    }

    private static (int StatusCode, string Code) Map(Exception exception)
    {
        var postgresException = FindPostgresException(exception);

        if (postgresException is not null)
        {
            return MapPostgres(postgresException);
        }

        return exception switch
        {
            BadHttpRequestException badRequest => (badRequest.StatusCode, ProblemFields.BadRequest),
            HttpRequestException => (StatusCodes.Status503ServiceUnavailable, nameof(ErrorCode.VaultwardenUnavailable)),
            TaskCanceledException => (StatusCodes.Status503ServiceUnavailable, nameof(ErrorCode.VaultwardenUnavailable)),
            DbUpdateException => (StatusCodes.Status503ServiceUnavailable, nameof(ErrorCode.DatabaseUnavailable)),
            _ => (StatusCodes.Status500InternalServerError, ProblemFields.InternalError)
        };
    }

    private static (int StatusCode, string Code) MapPostgres(PostgresException exception)
    {
        return exception.SqlState switch
        {
            PostgresErrorCodes.UniqueViolation => (StatusCodes.Status409Conflict, ProblemFields.Conflict),
            PostgresErrorCodes.NotNullViolation or PostgresErrorCodes.ForeignKeyViolation or PostgresErrorCodes.CheckViolation
                => (StatusCodes.Status400BadRequest, nameof(ErrorCode.InvalidSecretData)),
            _ => (StatusCodes.Status503ServiceUnavailable, nameof(ErrorCode.DatabaseUnavailable))
        };
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
