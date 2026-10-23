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
    private readonly ISessionRepository _sessionRepository;
    private readonly ISessionService _sessionService;
    private readonly ILocalizer _localizer;

    public StartHandler(ISessionRepository sessionRepository, ISessionService sessionService, ILocalizer localizer)
    {
        _sessionRepository = sessionRepository;
        _sessionService = sessionService;
        _localizer = localizer;
    }

    public async Task<Result<TelegramMessageContent>> Handle(StartMessage request, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetSession(request.TelegramUserId, cancellationToken);

        if (session is null)
        {
            return Result<TelegramMessageContent>.Success(new TelegramMessageContent
            {
                Text = _localizer.GetPhrase(WorkflowStep.Idle, LocalizationKeysConstants.Idle.PhoneRequest),
                RequestContact = true
            });
        }

        if (session.Roles.Count == 0)
        {
            return Result<TelegramMessageContent>.Success(new TelegramMessageContent
            {
                Text = _localizer.GetPhrase(WorkflowStep.Idle, LocalizationKeysConstants.Idle.NoServices)
            });
        }

        session.State = WorkflowStep.ServiceSelection;
        await _sessionService.SaveSession(session, cancellationToken);

        return Result<TelegramMessageContent>.Success(ServiceMenu.Create(session.Roles, _localizer));
    }
}
