using Atlas.Application.Common;
using Atlas.Domain.Constants.Localization;
using Atlas.Domain.Enums;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.ProblemCategorySelection;

public sealed class ProblemCategorySelectionHandler : IRequestHandler<ProblemCategorySelectionMessage, Result<WorkflowResponse>>
{
    private readonly ILocalizer _localizer;

    public ProblemCategorySelectionHandler(ILocalizer localizer)
    {
        _localizer = localizer;
    }

    public Task<Result<WorkflowResponse>> Handle(ProblemCategorySelectionMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<WorkflowResponse>.Success(CreateMenu()));
    }

    private WorkflowResponse CreateMenu()
    {
        return new WorkflowResponse
        {
            Text = _localizer.GetPhrase(
                nameof(WorkflowStep.ProblemCategorySelection),
                LocalizationKeysConstants.ProblemCategorySelection.Text),
            ButtonRows =
            [
                new WorkflowButtonRow(
                    new WorkflowButton(
                        _localizer.GetPhrase(
                            nameof(WorkflowStep.ProblemCategorySelection),
                            LocalizationKeysConstants.ProblemCategorySelection.Hardware),
                        CallbackConstants.MenuProblems),
                        new WorkflowButton(
                        _localizer.GetPhrase(
                            nameof(WorkflowStep.ProblemCategorySelection),
                            LocalizationKeysConstants.ProblemCategorySelection.Software),
                        CallbackConstants.MenuProblems),
                    new WorkflowButton(
                        _localizer.GetPhrase(
                            nameof(WorkflowStep.ProblemCategorySelection),
                            LocalizationKeysConstants.ProblemCategorySelection.Other),
                        CallbackConstants.MenuProfile))
            ]
        };
    }
}
