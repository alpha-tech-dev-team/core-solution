using CoreSolution.Shared.Results;
using MediatR;
using Pandora.TgBot.Application.Common.Interfaces;
using Pandora.TgBot.Application.Common.Services;

namespace Pandora.TgBot.Application.Behaviors;

public sealed class UserActivityBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly UserActivityAuditLogger _auditLogger;

    public UserActivityBehavior(UserActivityAuditLogger auditLogger)
    {
        _auditLogger = auditLogger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);

        if (request is IUserActionMessage userAction && response.IsSuccess && !cancellationToken.IsCancellationRequested)
        {
            await _auditLogger.Log(userAction, cancellationToken);
        }

        return response;
    }
}
