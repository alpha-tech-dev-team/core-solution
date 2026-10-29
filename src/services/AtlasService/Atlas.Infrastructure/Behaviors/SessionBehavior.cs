using Atlas.Core.Constants;
using Atlas.Core.Constants.Localization;
using Atlas.Core.Entities;
using Atlas.Core.Enums;
using Atlas.Core.Interfaces;
using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Atlas.Infrastructure.Behaviors;

public sealed class SessionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly IRequestContext _requestContext;
    private readonly ISessionService _sessionService;
    private readonly IIdentityClient _identityClient;
    private readonly ILogger<SessionBehavior<TRequest, TResponse>> _logger;

    public SessionBehavior(
        IRequestContext requestContext,
        ISessionService sessionService,
        IIdentityClient identityClient,
        ILogger<SessionBehavior<TRequest, TResponse>> logger)
    {
        _requestContext = requestContext;
        _sessionService = sessionService;
        _identityClient = identityClient;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IAuthorizedBotRequest botRequest && _requestContext.Session is null)
        {
            var session = await _sessionService.GetSession(botRequest.TelegramUserId, cancellationToken);

            if (session is null)
            {
                if (!await _identityClient.CheckUser(botRequest.TelegramUserId, AtlasConstants.ServiceName, cancellationToken))
                {
                    _logger.LogWarning("Доступ запрещён для пользователя {TelegramUserId}", botRequest.TelegramUserId);
                    return ResultFactory.Failure<TResponse>(ErrorCode.AccessDenied.ToString());
                }

                session = new UserSession(
                    botRequest.TelegramUserId,
                    WorkflowStep.ProblemCategorySelection,
                    LocalizationConstants.DefaultLanguage,
                    DateTime.UtcNow);

                await _sessionService.SaveSession(session, cancellationToken);
                _logger.LogInformation(
                    "Создана сессия для пользователя {TelegramUserId} со шагом {Step}",
                    botRequest.TelegramUserId,
                    session.State);
            }

            _requestContext.Session = session;
        }

        return await next(cancellationToken);
    }
}
