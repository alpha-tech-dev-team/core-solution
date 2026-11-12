using Atlas.Application.Common;
using Atlas.Domain.Constants;
using Atlas.Domain.Constants.Localization;
using Atlas.Domain.Enums;
using Atlas.Domain.Interfaces.Repositories;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Menu;

public sealed class ShowMenuHandler : IRequestHandler<ShowMenuMessage, Result<WorkflowResponse>>
{
    private readonly IIncidentCategoryRepository _categoryRepository;
    private readonly ILocalizer _localizer;

    public ShowMenuHandler(IIncidentCategoryRepository categoryRepository, ILocalizer localizer)
    {
        _categoryRepository = categoryRepository;
        _localizer = localizer;
    }

    public async Task<Result<WorkflowResponse>> Handle(ShowMenuMessage request, CancellationToken cancellationToken)
    {
        var page = await _categoryRepository.GetPageAsync(
            request.Page,
            PaginationConstants.DefaultPageSize,
            null,
            cancellationToken);

        if (page.Items.Count == 0)
        {
            return Result<WorkflowResponse>.Failure(nameof(ErrorCode.IncidentCategoriesNotFound));
        }

        var buttonRows = page.Items
            .Select(category => new WorkflowButtonRow(
                new WorkflowButton(category.Name, CallbackConstants.CategoryPrefix + category.Id)))
            .ToList();

        if (page.HasNextPage)
        {
            buttonRows.Add(new WorkflowButtonRow(
                new WorkflowButton(
                    _localizer.GetPhrase(
                        nameof(WorkflowStep.ProblemCategorySelection),
                        LocalizationKeysConstants.ProblemCategorySelection.NextPage),
                    CallbackConstants.CategoryPagePrefix + (page.Page + 1))));
        }

        return Result<WorkflowResponse>.Success(new WorkflowResponse
        {
            Text = _localizer.GetPhrase(
                nameof(WorkflowStep.ProblemCategorySelection),
                LocalizationKeysConstants.ProblemCategorySelection.Text),
            ButtonRows = buttonRows
        });
    }
}
