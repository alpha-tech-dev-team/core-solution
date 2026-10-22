using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Core.Entities;
using Pandora.Core.Enums;
using Pandora.Core.Interfaces;
using Pandora.Infrastructure.Utils;

namespace Pandora.Infrastructure.Behaviors;

public sealed class SessionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly IRequestContext _requestContext;
    private readonly ISessionRepository _sessionRepository;
    private readonly ISessionService _sessionService;

    public SessionBehavior(IRequestContext requestContext, ISessionRepository sessionRepository, ISessionService sessionService)
    {
        _requestContext = requestContext;
        _sessionRepository = sessionRepository;
        _sessionService = sessionService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IAuthorizedBotRequest botRequest && _requestContext.Session is null)
        {
            var session = await _sessionRepository.GetSession(botRequest.ChatId, cancellationToken);

            if (session is null)
            {
                session = new UserSession
                {
                    Id = botRequest.ChatId,
                    State = WorkflowStep.Idle,
                    LastUpdate = DateTime.UtcNow
                };

                await _sessionService.SaveSession(session, cancellationToken);
            }

            _requestContext.Session = session;
        }

        return await next(cancellationToken);
    }
}
