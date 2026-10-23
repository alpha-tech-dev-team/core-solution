using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Pandora.Core.Enums;
using Pandora.Core.Interfaces;

namespace Pandora.Infrastructure.Behaviors;

public sealed class SessionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly IRequestContext _requestContext;
    private readonly ISessionService _sessionService;
    private readonly ILogger<SessionBehavior<TRequest, TResponse>> _logger;

    public SessionBehavior(IRequestContext requestContext, ISessionService sessionService, ILogger<SessionBehavior<TRequest, TResponse>> logger)
    {
        _requestContext = requestContext;
        _sessionService = sessionService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IAuthorizedBotRequest botRequest && _requestContext.Session is null)
        {
            var session = await _sessionService.GetSession(botRequest.TelegramUserId, cancellationToken);

            if (session is null)
            {
                _logger.LogWarning("Сессия не найдена, сообщение {MessageId}", botRequest.MessageId);

                return ResultFactory.Failure<TResponse>(ErrorCode.SessionNotFound.ToString());
            }

            _requestContext.Session = session;
        }

        return await next(cancellationToken);
    }
}
