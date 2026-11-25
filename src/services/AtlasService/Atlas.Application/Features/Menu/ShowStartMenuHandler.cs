using Atlas.Application.Common;
using Atlas.Domain.Constants.Localization;
using Atlas.Domain.Enums;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Menu;

public sealed class ShowStartMenuHandler : IRequestHandler<ShowStartMenuMessage, Result<WorkflowResponse>>
{
    private readonly ILocalizer _localizer;

    public ShowStartMenuHandler(ILocalizer localizer)
    {
        _localizer = localizer;
    }

    public Task<Result<WorkflowResponse>> Handle(ShowStartMenuMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<WorkflowResponse>.Success(new WorkflowResponse
        {
            Text = _localizer.GetPhrase(
                nameof(WorkflowStep.Menu),
                LocalizationKeysConstants.Menu.Text),
            ButtonRows = new List<WorkflowButtonRow>
            {
                new(new WorkflowButton(
                    _localizer.GetPhrase(
                        nameof(WorkflowStep.Menu),
                        LocalizationKeysConstants.Menu.CreateIncident),
                    CallbackConstants.IncidentPrefix + IncidentStepAction.Create))
            }
        }));
    }
}
