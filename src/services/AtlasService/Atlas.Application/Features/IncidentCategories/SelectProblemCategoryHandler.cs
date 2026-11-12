using Atlas.Domain.Constants.Localization;
using Atlas.Domain.Enums;
using Atlas.Domain.Interfaces;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Domain.Models;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.IncidentCategories;

public sealed class SelectProblemCategoryHandler : IRequestHandler<SelectProblemCategoryMessage, Result<WorkflowResponse>>
{
    private readonly IIncidentCategoryRepository _categoryRepository;
    private readonly IDraftStore _draftStore;
    private readonly ILocalizer _localizer;

    public SelectProblemCategoryHandler(
        IIncidentCategoryRepository categoryRepository,
        IDraftStore draftStore,
        ILocalizer localizer)
    {
        _categoryRepository = categoryRepository;
        _draftStore = draftStore;
        _localizer = localizer;
    }

    public async Task<Result<WorkflowResponse>> Handle(SelectProblemCategoryMessage request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);

        if (category is null)
        {
            return Result<WorkflowResponse>.Failure(nameof(ErrorCode.IncidentCategoryNotFound));
        }

        var draft = await _draftStore.GetAsync(request.TelegramUserId, cancellationToken)
            ?? new IncidentDraft();

        draft.Step = WorkflowStep.CategorySelected;
        draft.CategoryId = category.Id;
        draft.UpdatedAt = DateTime.UtcNow;

        await _draftStore.SaveAsync(request.TelegramUserId, draft, cancellationToken);

        return Result<WorkflowResponse>.Success(new WorkflowResponse
        {
            Text = string.Format(
                _localizer.GetPhrase(
                    nameof(WorkflowStep.CategorySelected),
                    LocalizationKeysConstants.CategorySelected.Text),
                category.Name)
        });
    }
}
