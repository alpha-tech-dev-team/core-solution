using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Pandora.Api.Constants;

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

        var (statusCode, code) = PandoraErrorStatusCodes.Map(exception);

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
}
