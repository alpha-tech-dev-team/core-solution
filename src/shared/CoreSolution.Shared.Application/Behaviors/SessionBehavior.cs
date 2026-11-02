using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Constants;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreSolution.Shared.Application.Behaviors;

public sealed class SessionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly IRequestContext _requestContext;
    private readonly ISessionService _sessionService;
    private readonly IIdentityClient _identityClient;
    private readonly ILocalizer _localizer;
    private readonly SessionBehaviorOptions _options;
    private readonly ILogger<SessionBehavior<TRequest, TResponse>> _logger;

    public SessionBehavior(
        IRequestContext requestContext,
        ISessionService sessionService,
        IIdentityClient identityClient,
        ILocalizer localizer,
        SessionBehaviorOptions options,
        ILogger<SessionBehavior<TRequest, TResponse>> logger)
    {
        _requestContext = requestContext;
        _sessionService = sessionService;
        _identityClient = identityClient;
        _localizer = localizer;
        _options = options;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IAuthorizedBotRequest botRequest && _requestContext.Session is null)
        {
            var session = await _sessionService.GetSession(botRequest.TelegramUserId, cancellationToken);

            if (session is null)
            {
                if (!await _identityClient.CheckUser(botRequest.TelegramUserId, _options.ServiceName, cancellationToken))
                {
                    _logger.LogWarning(
                        "Identity не подтвердил доступ, запрошен номер телефона, пользователь {TgUserId}, сообщение {MessageId}",
                        botRequest.TelegramUserId,
                        botRequest.MessageId);

                    return ResultFactory.Success<TResponse, WorkflowResponse>(new WorkflowResponse
                    {
                        Text = _localizer.GetPhrase(LocalizationConstants.IdleStateName, LocalizationConstants.Idle.PhoneRequest),
                        RequestContact = true
                    });
                }

                session = UserSession.Create(botRequest.TelegramUserId, _options.InitialStep);
                await _sessionService.SaveSession(session, cancellationToken);

                _logger.LogInformation(
                    "Сессия восстановлена по identity, пользователь {TgUserId}, сообщение {MessageId}",
                    botRequest.TelegramUserId,
                    botRequest.MessageId);
            }

            _requestContext.Session = session;
        }

        return await next(cancellationToken);
    }
}
