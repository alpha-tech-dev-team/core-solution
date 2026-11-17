using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Application.Features.Activation;
using CoreSolution.Shared.Application.Messages;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Constants;
using Pandora.Domain.Enums;
using Pandora.TgBot.Application.Common.Services;
using Pandora.TgBot.Application.Features.Services;

namespace Pandora.TgBot.Application.Features.Activation;

public sealed class SubmitPhoneHandler : SubmitPhoneHandlerBase
{
    private readonly UserActivityAuditLogger _auditLogger;

    public SubmitPhoneHandler(
        IPhoneConfirmationService phoneConfirmationService,
        IMediator mediator,
        ILocalizer localizer,
        UserActivityAuditLogger auditLogger)
        : base(phoneConfirmationService, mediator, localizer, nameof(WorkflowStep.ServiceSelection))
    {
        _auditLogger = auditLogger;
    }

    public override async Task<Result<WorkflowResponse>> Handle(SubmitPhoneMessage request, CancellationToken cancellationToken)
    {
        var result = await base.Handle(request, cancellationToken);

        if (result.IsSuccess && request.Phone is not null)
        {
            await _auditLogger.Log(request.TelegramUserId, AuditConstants.Actions.SubmitPhone, targetItemId: null, cancellationToken);
        }

        return result;
    }

    protected override IRequest<Result<WorkflowResponse>> CreateNextMessage(SubmitPhoneMessage request)
    {
        return new ShowServicesMessage(request.TelegramUserId, request.MessageId);
    }
}
