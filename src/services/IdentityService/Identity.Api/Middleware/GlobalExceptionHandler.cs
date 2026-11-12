using Identity.Api.Constants;
using Identity.Api.Models;
using Identity.Application.Errors;
using Microsoft.AspNetCore.Diagnostics;

namespace Identity.Api.Middleware;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted)
        {
            return false;
        }

        if (UniqueConstraintErrorCodes.TryGetErrorCode(exception, out var errorCode))
        {
            _logger.LogWarning("Нарушение уникальности при сохранении {Method} {Path}: {ErrorCode}", httpContext.Request.Method, httpContext.Request.Path, errorCode);

            httpContext.Response.StatusCode = IdentityErrorStatusCodes.Get(errorCode);
            await httpContext.Response.WriteAsJsonAsync(new ErrorResponse(errorCode), cancellationToken);

            return true;
        }

        _logger.LogError(exception, "Необработанное исключение при обработке {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

        var code = nameof(IdentityErrorCode.InternalError);
        httpContext.Response.StatusCode = IdentityErrorStatusCodes.Get(code);

        await httpContext.Response.WriteAsJsonAsync(new ErrorResponse(code), cancellationToken);

        return true;
    }
}
