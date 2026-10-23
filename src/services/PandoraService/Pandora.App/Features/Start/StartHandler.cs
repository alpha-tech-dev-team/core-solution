using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.App.Common;
using Pandora.Core.Constants.Localization;
using Pandora.Core.Enums;
using Pandora.Core.Interfaces;

namespace Pandora.App.Features.Start;

public sealed class StartHandler : IRequestHandler<StartMessage, Result<TelegramMessageContent>>
{
    private readonly ISessionService _sessionService;
    private readonly ISecretStore _secretStore;
    private readonly ILocalizer _localizer;

    public StartHandler(ISessionService sessionService, ISecretStore secretStore, ILocalizer localizer)
    {
        _sessionService = sessionService;
        _secretStore = secretStore;
        _localizer = localizer;
    }

    public async Task<Result<TelegramMessageContent>> Handle(StartMessage request, CancellationToken cancellationToken)
    {
        var session = await _sessionService.GetSession(request.TelegramUserId, cancellationToken);

        if (session is null)
        {
            return Result<TelegramMessageContent>.Success(new TelegramMessageContent
            {
                Text = _localizer.GetPhrase(WorkflowStep.Idle, LocalizationKeysConstants.Idle.PhoneRequest),
                RequestContact = true
            });
        }

        var services = _secretStore.GetServiceNames();

        if (services.Count == 0)
        {
            session.State = WorkflowStep.Idle;
            await _sessionService.SaveSession(session, cancellationToken);

            return Result<TelegramMessageContent>.Success(new TelegramMessageContent
            {
                Text = _localizer.GetPhrase(WorkflowStep.Idle, LocalizationKeysConstants.Idle.NoServices)
            });
        }

        session.State = WorkflowStep.ServiceSelection;
        await _sessionService.SaveSession(session, cancellationToken);

        return Result<TelegramMessageContent>.Success(ServiceMenu.Create(services, _localizer));
    }
}
