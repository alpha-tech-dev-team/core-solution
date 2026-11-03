using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Application.Features.Activation;
using CoreSolution.Shared.Application.Messages;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Application.Features.Services;
using Pandora.Domain.Enums;

namespace Pandora.Application.Features.Activation;

public sealed class SubmitPhoneHandler : SubmitPhoneHandlerBase
{
    public SubmitPhoneHandler(IPhoneConfirmationService phoneConfirmationService, IMediator mediator, ILocalizer localizer)
        : base(phoneConfirmationService, mediator, localizer, nameof(WorkflowStep.ServiceSelection))
    {
    }

    protected override IRequest<Result<WorkflowResponse>> CreateNextMessage(SubmitPhoneMessage request)
    {
        return new ShowServicesMessage(request.TelegramUserId, request.MessageId);
    }
}
