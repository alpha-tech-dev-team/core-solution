using Atlas.Application.Common;
using Atlas.Domain.Constants.Localization;
using Atlas.Domain.Enums;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Menu;

public sealed class ShowMenuHandler : IRequestHandler<ShowMenuMessage, Result<WorkflowResponse>>
{
    private readonly ILocalizer _localizer;

    public ShowMenuHandler(ILocalizer localizer)
    {
        _localizer = localizer;
    }

    public Task<Result<WorkflowResponse>> Handle(ShowMenuMessage request, CancellationToken cancellationToken)
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
                            LocalizationKeysConstants.ProblemCategorySelection.Problems),
                        CallbackConstants.MenuProblems),
                    new WorkflowButton(
                        _localizer.GetPhrase(
                            nameof(WorkflowStep.ProblemCategorySelection),
                            LocalizationKeysConstants.ProblemCategorySelection.Profile),
                        CallbackConstants.MenuProfile))
            ]
        };
    }
}
