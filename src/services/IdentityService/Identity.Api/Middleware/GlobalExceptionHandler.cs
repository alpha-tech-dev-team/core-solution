using Identity.Api.Constants;
using Identity.Api.Models;
using Identity.Application.Errors;
using Microsoft.AspNetCore.Diagnostics;

namespace Identity.Api.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted)
        {
            return false;
        }

        logger.LogError(
            exception,
            "Необработанное исключение при обработке {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        var code = nameof(IdentityErrorCode.InternalError);
        httpContext.Response.StatusCode = IdentityErrorStatusCodes.Get(code);

        await httpContext.Response.WriteAsJsonAsync(new ErrorResponse(code), cancellationToken);

        return true;
    }
}
