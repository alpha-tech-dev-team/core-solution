using Atlas.Application.Features.Menu;
using Atlas.Domain.Constants;
using Atlas.Domain.Enums;
using Atlas.Domain.Models;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Application.Messages;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.TgBot.Features.Activation;

public sealed class SkipSubmitPhoneHandler : IRequestHandler<SubmitPhoneMessage, Result<WorkflowResponse>>
{
    private readonly IMediator _mediator;
    private readonly ISessionService _sessionService;

    public SkipSubmitPhoneHandler(IMediator mediator, ISessionService sessionService)
    {
        _mediator = mediator;
        _sessionService = sessionService;
    }

    public async Task<Result<WorkflowResponse>> Handle(SubmitPhoneMessage request, CancellationToken cancellationToken)
    {
        await _sessionService.SaveSession(
            AtlasUserSession.Create(request.TelegramUserId, nameof(WorkflowStep.ProblemCategorySelection)),
            cancellationToken);

        return await _mediator.Send(
            new ShowMenuMessage(request.TelegramUserId, request.MessageId, PaginationLimits.FirstPage),
            cancellationToken);
    }
}
