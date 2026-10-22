using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.App.Common;
using Pandora.Core.Constants;
using Pandora.Core.Enums;
using Pandora.Core.Interfaces;

namespace Pandora.App.Features.Start;

public sealed class StartHandler : IRequestHandler<StartMessage, Result<TelegramMessageContent>>
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ISessionService _sessionService;

    public StartHandler(ISessionRepository sessionRepository, ISessionService sessionService)
    {
        _sessionRepository = sessionRepository;
        _sessionService = sessionService;
    }

    public async Task<Result<TelegramMessageContent>> Handle(StartMessage request, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetSession(request.TelegramUserId, cancellationToken);

        if (session is null)
        {
            return Result<TelegramMessageContent>.Success(new TelegramMessageContent
            {
                Text = StartConstants.PhoneRequestText,
                RequestContact = true
            });
        }

        if (session.Roles.Count == 0)
        {
            return Result<TelegramMessageContent>.Success(new TelegramMessageContent { Text = SessionConstants.NoServicesText });
        }

        session.State = WorkflowStep.ServiceSelection;
        await _sessionService.SaveSession(session, cancellationToken);

        return Result<TelegramMessageContent>.Success(ServiceMenu.Create(session.Roles));
    }
}
