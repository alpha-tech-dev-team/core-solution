using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Pandora.Api.Constants;
using Pandora.Domain.Enums;

namespace Pandora.Api.Middleware;

public sealed class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            if (exception is TaskCanceledException canceled && canceled.CancellationToken == context.RequestAborted)
            {
                return;
            }

            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Необработанное исключение при {Method} {Path}", context.Request.Method, context.Request.Path);

        var (statusCode, code) = exception switch
        {
            HttpRequestException => (StatusCodes.Status503ServiceUnavailable, nameof(ErrorCode.VaultwardenUnavailable)),
            TaskCanceledException => (StatusCodes.Status503ServiceUnavailable, nameof(ErrorCode.VaultwardenUnavailable)),
            DbUpdateException => (StatusCodes.Status503ServiceUnavailable, nameof(ErrorCode.DatabaseUnavailable)),
            PostgresException => (StatusCodes.Status503ServiceUnavailable, nameof(ErrorCode.DatabaseUnavailable)),
            _ => (StatusCodes.Status500InternalServerError, ProblemFields.InternalError)
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new
        {
            type = "about:blank",
            status = statusCode,
            code
        });
    }
}
