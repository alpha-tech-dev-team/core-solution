using Atlas.Application.Features.Menu;
using Atlas.Core.Enums;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Application.Features.Activation;
using CoreSolution.Shared.Application.Messages;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Activation;

public sealed class SubmitPhoneHandler : SubmitPhoneHandlerBase
{
    public SubmitPhoneHandler(IPhoneConfirmationService phoneConfirmationService, IMediator mediator, ILocalizer localizer)
        : base(phoneConfirmationService, mediator, localizer, nameof(WorkflowStep.ProblemCategorySelection))
    {
    }

    protected override IRequest<Result<WorkflowResponse>> CreateNextMessage(SubmitPhoneMessage request)
    {
        return new ShowMenuMessage(request.TelegramUserId, request.MessageId);
    }
}
