using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.App.Common;
using Pandora.Core.Constants;
using Pandora.Core.Enums;
using Pandora.Core.Interfaces;

namespace Pandora.App.Features.Activation;

public sealed class SubmitPhoneHandler : IRequestHandler<SubmitPhoneMessage, Result<TelegramMessageContent>>
{
    private readonly IRequestContext _requestContext;
    private readonly ISessionService _sessionService;

    public SubmitPhoneHandler(IRequestContext requestContext, ISessionService sessionService)
    {
        _requestContext = requestContext;
        _sessionService = sessionService;
    }

    public async Task<Result<TelegramMessageContent>> Handle(SubmitPhoneMessage request, CancellationToken cancellationToken)
    {
        var session = _requestContext.Session;

        if (session.Roles.Count == 0)
        {
            return Result<TelegramMessageContent>.Success(new TelegramMessageContent { Text = SessionConstants.NoServicesText });
        }

        session.State = WorkflowStep.ServiceSelection;
        await _sessionService.SaveSession(session, cancellationToken);

        return Result<TelegramMessageContent>.Success(ServiceMenu.Create(session.Roles));
    }
}
