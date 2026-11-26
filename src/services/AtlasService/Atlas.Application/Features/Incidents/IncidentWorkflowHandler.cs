using Atlas.Application.Common;
using Atlas.Application.Features.Menu;
using Atlas.Domain.Constants;
using Atlas.Domain.Constants.Localization;
using Atlas.Domain.Enums;
using Atlas.Domain.Models;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Incidents;

public sealed class IncidentWorkflowHandler : IRequestHandler<IncidentWorkflowMessage, Result<WorkflowResponse>>
{
    private readonly IncidentCreationService _creationService;
    private readonly IncidentConfirmationBuilder _confirmationBuilder;
    private readonly IRequestContext _requestContext;
    private readonly ISessionService _sessionService;
    private readonly ILocalizer _localizer;
    private readonly IMediator _mediator;

    public IncidentWorkflowHandler(
        IncidentCreationService creationService,
        IncidentConfirmationBuilder confirmationBuilder,
        IRequestContext requestContext,
        ISessionService sessionService,
        ILocalizer localizer,
        IMediator mediator)
    {
        _creationService = creationService;
        _confirmationBuilder = confirmationBuilder;
        _requestContext = requestContext;
        _sessionService = sessionService;
        _localizer = localizer;
        _mediator = mediator;
    }

    public async Task<Result<WorkflowResponse>> Handle(IncidentWorkflowMessage request, CancellationToken cancellationToken)
    {
        var session = (AtlasUserSession)_requestContext.Session;

        if (request.Action == IncidentStepAction.Create)
        {
            return await StartAsync(request, cancellationToken);
        }

        return session.Step switch
        {
            WorkflowStep.IncidentDescription => await SaveDescriptionAsync(session, request, cancellationToken),
            WorkflowStep.IncidentConfirmation => await HandleConfirmationAsync(session, request, cancellationToken),
            WorkflowStep.IncidentCreated => await RestartAsync(session, request, cancellationToken),
            _ => Result<WorkflowResponse>.Failure(nameof(ErrorCode.IncidentStepNotSupported))
        };
    }

    private async Task<Result<WorkflowResponse>> StartAsync(
        IncidentWorkflowMessage request,
        CancellationToken cancellationToken)
    {
        return await _mediator.Send(
            new ShowMenuMessage(request.TelegramUserId, request.MessageId, PaginationLimits.FirstPage),
            cancellationToken);
    }

    private async Task<Result<WorkflowResponse>> SaveDescriptionAsync(
        AtlasUserSession session,
        IncidentWorkflowMessage request,
        CancellationToken cancellationToken)
    {
        var description = request.Text?.Trim();

        if (string.IsNullOrWhiteSpace(description)
            || description.Length > ValidationConstants.IncidentDescriptionMaxLength)
        {
            return Result<WorkflowResponse>.Failure(nameof(ErrorCode.InvalidIncidentDescription));
        }

        session.IncidentDescription = description;

        await _sessionService.UpdateState(session, nameof(WorkflowStep.IncidentConfirmation), cancellationToken);

        var confirmation = await _confirmationBuilder.BuildAsync(session, cancellationToken);

        return Result<WorkflowResponse>.Success(confirmation);
    }

    private Task<Result<WorkflowResponse>> HandleConfirmationAsync(
        AtlasUserSession session,
        IncidentWorkflowMessage request,
        CancellationToken cancellationToken)
    {
        return request.Action switch
        {
            IncidentStepAction.Confirm => ConfirmAsync(session, request, cancellationToken),
            IncidentStepAction.Cancel => CancelAsync(session, request, cancellationToken),
            _ => Task.FromResult(Result<WorkflowResponse>.Failure(nameof(ErrorCode.IncidentStepNotSupported)))
        };
    }

    private async Task<Result<WorkflowResponse>> ConfirmAsync(
        AtlasUserSession session,
        IncidentWorkflowMessage request,
        CancellationToken cancellationToken)
    {
        var created = await _creationService.CreateAsync(
            session,
            request.TelegramUserId,
            request.EmployeeName,
            request.EmployeeSurname,
            cancellationToken);

        if (!created.IsSuccess)
        {
            return Result<WorkflowResponse>.Failure(created.ErrorCode!);
        }

        var ticketNumber = created.Value!.TicketNumber;

        session.ResetIncident();

        await _sessionService.UpdateState(session, nameof(WorkflowStep.IncidentCreated), cancellationToken);

        return Result<WorkflowResponse>.Success(new WorkflowResponse
        {
            Text = string.Format(
                _localizer.GetPhrase(
                    nameof(WorkflowStep.IncidentCreated),
                    LocalizationKeysConstants.IncidentCreated.Text),
                ticketNumber),
            ButtonRows = new List<WorkflowButtonRow>
            {
                new(new WorkflowButton(
                    _localizer.GetPhrase(
                        nameof(WorkflowStep.IncidentCreated),
                        LocalizationKeysConstants.IncidentCreated.Button),
                    CallbackConstants.IncidentPrefix + IncidentStepAction.Restart))
            }
        });
    }

    private async Task<Result<WorkflowResponse>> RestartAsync(
        AtlasUserSession session,
        IncidentWorkflowMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Action is not IncidentStepAction.Restart)
        {
            return Result<WorkflowResponse>.Failure(nameof(ErrorCode.IncidentStepNotSupported));
        }

        session.ResetIncident();

        await _sessionService.UpdateState(session, nameof(WorkflowStep.Menu), cancellationToken);

        return await _mediator.Send(
            new ShowStartMenuMessage(request.TelegramUserId, request.MessageId),
            cancellationToken);
    }

    private async Task<Result<WorkflowResponse>> CancelAsync(
        AtlasUserSession session,
        IncidentWorkflowMessage request,
        CancellationToken cancellationToken)
    {
        session.ResetIncident();

        await _sessionService.UpdateState(session, nameof(WorkflowStep.Menu), cancellationToken);

        var menu = await _mediator.Send(
            new ShowStartMenuMessage(request.TelegramUserId, request.MessageId),
            cancellationToken);

        if (!menu.IsSuccess)
        {
            return menu;
        }

        return Result<WorkflowResponse>.Success(new WorkflowResponse
        {
            Text = _localizer.GetPhrase(
                nameof(WorkflowStep.IncidentConfirmation),
                LocalizationKeysConstants.IncidentConfirmation.CancelledText),
            ButtonRows = menu.Value!.ButtonRows
        });
    }
}
