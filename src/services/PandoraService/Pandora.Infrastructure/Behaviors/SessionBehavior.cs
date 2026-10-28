using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Pandora.Core.Constants;
using Pandora.Core.Constants.Localization;
using Pandora.Core.Entities;
using Pandora.Core.Enums;
using Pandora.Core.Interfaces;

namespace Pandora.Infrastructure.Behaviors;

public sealed class SessionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly IRequestContext _requestContext;
    private readonly ISessionService _sessionService;
    private readonly IIdentityClient _identityClient;
    private readonly ILocalizer _localizer;
    private readonly ILogger<SessionBehavior<TRequest, TResponse>> _logger;

    public SessionBehavior(IRequestContext requestContext, ISessionService sessionService, IIdentityClient identityClient, ILocalizer localizer, ILogger<SessionBehavior<TRequest, TResponse>> logger)
    {
        _requestContext = requestContext;
        _sessionService = sessionService;
        _identityClient = identityClient;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IAuthorizedBotRequest botRequest && _requestContext.Session is null)
        {
            var session = await _sessionService.GetSession(botRequest.TelegramUserId, cancellationToken);

            if (session is null)
            {
                if (!await _identityClient.CheckUser(botRequest.TelegramUserId, PandoraConstants.ServiceName, cancellationToken))
                {
                    _logger.LogWarning("Identity не подтвердил доступ, запрошен номер телефона, сообщение {MessageId}", botRequest.MessageId);

                    return ResultFactory.Success<TResponse, TelegramMessageContent>(new TelegramMessageContent
                    {
                        Text = _localizer.GetPhrase(WorkflowStep.Idle, LocalizationKeysConstants.Idle.PhoneRequest),
                        RequestContact = true
                    });
                }

                session = new UserSession(botRequest.TelegramUserId, WorkflowStep.ServiceSelection, LocalizationConstants.DefaultLanguage, DateTime.UtcNow);
                await _sessionService.SaveSession(session, cancellationToken);

                _logger.LogInformation("Сессия восстановлена по identity, сообщение {MessageId}", botRequest.MessageId);
            }

            _requestContext.Session = session;
        }

        return await next(cancellationToken);
    }
}
