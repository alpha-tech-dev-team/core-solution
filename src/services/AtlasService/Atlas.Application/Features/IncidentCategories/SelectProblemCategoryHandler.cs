using Atlas.Application.Common;
using Atlas.Domain.Constants;
using Atlas.Domain.Constants.Localization;
using Atlas.Domain.Enums;
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
    private readonly IRequestContext _requestContext;
    private readonly ISessionService _sessionService;
    private readonly ILocalizer _localizer;

    public SelectProblemCategoryHandler(
        IIncidentCategoryRepository categoryRepository,
        IRequestContext requestContext,
        ISessionService sessionService,
        ILocalizer localizer)
    {
        _categoryRepository = categoryRepository;
        _requestContext = requestContext;
        _sessionService = sessionService;
        _localizer = localizer;
    }

    public async Task<Result<WorkflowResponse>> Handle(SelectProblemCategoryMessage request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);

        if (category is null)
        {
            return Result<WorkflowResponse>.Failure(nameof(ErrorCode.IncidentCategoryNotFound));
        }

        var session = (AtlasUserSession)_requestContext.Session;
        session.IncidentCategoryId = category.Id;

        await _sessionService.UpdateState(session, nameof(WorkflowStep.CategorySelected), cancellationToken);

        return Result<WorkflowResponse>.Success(new WorkflowResponse
        {
            Text = string.Format(
                _localizer.GetPhrase(
                    nameof(WorkflowStep.CategorySelected),
                    LocalizationKeysConstants.CategorySelected.Text),
                category.Name),
            ButtonRows = new List<WorkflowButtonRow>
            {
                new(new WorkflowButton(
                    _localizer.GetPhrase(
                        nameof(WorkflowStep.CategorySelected),
                        LocalizationKeysConstants.CategorySelected.Button),
                    CallbackConstants.CategoryPagePrefix + PaginationLimits.FirstPage))
            }
        });
    }
}
