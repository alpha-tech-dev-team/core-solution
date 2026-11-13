using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Application.Messages;
using CoreSolution.Shared.Constants;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace CoreSolution.Shared.Application.Features.Activation;

public abstract class SubmitPhoneHandlerBase : IRequestHandler<SubmitPhoneMessage, Result<WorkflowResponse>>
{
    private readonly IPhoneConfirmationService _phoneConfirmationService;
    private readonly IMediator _mediator;
    private readonly ILocalizer _localizer;
    private readonly string _initialStep;

    protected SubmitPhoneHandlerBase(
        IPhoneConfirmationService phoneConfirmationService,
        IMediator mediator,
        ILocalizer localizer,
        string initialStep)
    {
        _phoneConfirmationService = phoneConfirmationService;
        _mediator = mediator;
        _localizer = localizer;
        _initialStep = initialStep;
    }

    protected abstract IRequest<Result<WorkflowResponse>> CreateNextMessage(SubmitPhoneMessage request);

    public async Task<Result<WorkflowResponse>> Handle(SubmitPhoneMessage request, CancellationToken cancellationToken)
    {
        if (request.Phone is null)
        {
            return Result<WorkflowResponse>.Success(new WorkflowResponse
            {
                Text = _localizer.GetPhrase(LocalizationConstants.IdleStateName, LocalizationConstants.Idle.PhoneRequest),
                RequestContact = true,
                ContactButtonText = _localizer.GetPhrase(LocalizationConstants.IdleStateName, LocalizationConstants.Idle.BtnShareContact)
            });
        }

        var confirmationError = await _phoneConfirmationService.ConfirmAsync(
            request.TelegramUserId,
            request.Phone,
            _initialStep,
            cancellationToken);

        if (confirmationError is not null)
        {
            return Result<WorkflowResponse>.Failure(
                confirmationError == ErrorCodes.UserBlocked ? ErrorCodes.UserBlocked : ErrorCodes.AccessNotConfirmed);
        }

        return await _mediator.Send(CreateNextMessage(request), cancellationToken);
    }
}
