using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Pandora.Core.Constants;
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
    private readonly IIdentityClient _identityClient;
    private readonly ISecretStore _secretStore;
    private readonly ILogger<SessionBehavior<TRequest, TResponse>> _logger;

    public SessionBehavior(IRequestContext requestContext, ISessionRepository sessionRepository, ISessionService sessionService, IIdentityClient identityClient, ISecretStore secretStore, ILogger<SessionBehavior<TRequest, TResponse>> logger)
    {
        _requestContext = requestContext;
        _sessionRepository = sessionRepository;
        _sessionService = sessionService;
        _identityClient = identityClient;
        _secretStore = secretStore;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IActivatingBotRequest activating && _requestContext.Session is null)
        {
            var approved = await _identityClient.ActivateUser(activating.TelegramUserId, activating.Phone, cancellationToken);

            if (!approved)
            {
                _logger.LogWarning("Access denied for message {MessageId}", activating.MessageId);

                return ResultFactory.Failure<TResponse>(ErrorCode.AccessDenied.ToString(), [SessionConstants.AccessDeniedText]);
            }

            var session = new UserSession
            {
                TgUserId = activating.TelegramUserId,
                State = WorkflowStep.Idle,
                Roles = [.. await ResolveRoles(activating.TelegramUserId, cancellationToken)],
                LastUpdate = DateTime.UtcNow
            };

            await _sessionService.SaveSession(session, cancellationToken);
            _requestContext.Session = session;
        }
        else if (request is IAuthorizedBotRequest botRequest && _requestContext.Session is null)
        {
            var session = await _sessionRepository.GetSession(botRequest.TelegramUserId, cancellationToken);

            if (session is null)
            {
                _logger.LogWarning("Session not found for message {MessageId}", botRequest.MessageId);

                return ResultFactory.Failure<TResponse>(ErrorCode.AccessDenied.ToString(), [SessionConstants.RestartText]);
            }

            _requestContext.Session = session;
        }

        return await next(cancellationToken);
    }

    private async Task<IReadOnlyList<string>> ResolveRoles(long telegramUserId, CancellationToken cancellationToken)
    {
        var services = _secretStore.GetServiceNames();
        var roles = new List<string>(services.Count);

        foreach (var service in services)
        {
            if (await _identityClient.CheckUser(telegramUserId, service, cancellationToken))
            {
                roles.Add(service);
            }
        }

        return roles;
    }
}
