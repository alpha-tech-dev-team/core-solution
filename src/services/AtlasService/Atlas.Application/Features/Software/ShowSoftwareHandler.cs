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

namespace Atlas.Application.Features.Software;

public sealed class ShowSoftwareHandler : IRequestHandler<ShowSoftwareMessage, Result<WorkflowResponse>>
{
    private readonly ISoftwareRepository _softwareRepository;
    private readonly IIncidentCategoryRepository _categoryRepository;
    private readonly IRequestContext _requestContext;
    private readonly ISessionService _sessionService;
    private readonly ILocalizer _localizer;

    public ShowSoftwareHandler(
        ISoftwareRepository softwareRepository,
        IIncidentCategoryRepository categoryRepository,
        IRequestContext requestContext,
        ISessionService sessionService,
        ILocalizer localizer)
    {
        _softwareRepository = softwareRepository;
        _categoryRepository = categoryRepository;
        _requestContext = requestContext;
        _sessionService = sessionService;
        _localizer = localizer;
    }

    public async Task<Result<WorkflowResponse>> Handle(ShowSoftwareMessage request, CancellationToken cancellationToken)
    {
        var session = (AtlasUserSession)_requestContext.Session;

        if (session.RoomId is null || session.IncidentCategoryId is not Guid categoryId)
        {
            return Result<WorkflowResponse>.Failure(nameof(ErrorCode.IncidentContextIncomplete));
        }

        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);

        if (category?.Type != IncidentCategoryType.Software)
        {
            return Result<WorkflowResponse>.Failure(nameof(ErrorCode.IncidentContextIncomplete));
        }

        var page = await _softwareRepository.GetPageAsync(
            new PageRequest(request.Page, PaginationLimits.DefaultPageSize),
            query => query.OrderBy(item => item.Name).ThenBy(item => item.Version).ThenBy(item => item.Id),
            cancellationToken);

        await _sessionService.UpdateState(session, nameof(WorkflowStep.SoftwareSelection), cancellationToken);

        if (page.Items.Count == 0)
        {
            return Result<WorkflowResponse>.Failure(nameof(ErrorCode.SoftwareNotConfigured));
        }

        var rows = page.Items
            .Select(item => new WorkflowButtonRow(new WorkflowButton(
                item.Version is null
                    ? item.Name
                    : string.Format(SoftwareConstants.DisplayNameFormat, item.Name, item.Version),
                CallbackConstants.SoftwarePrefix + item.Id)))
            .ToList();

        if (page.HasNextPage)
        {
            rows.Add(new WorkflowButtonRow(new WorkflowButton(
                _localizer.GetPhrase(
                    nameof(WorkflowStep.SoftwareSelection),
                    LocalizationKeysConstants.SoftwareSelection.NextPage),
                CallbackConstants.SoftwarePagePrefix + page.NextPage)));
        }

        rows.AddRange(BuildNavigationRows());

        return Result<WorkflowResponse>.Success(new WorkflowResponse
        {
            Text = _localizer.GetPhrase(
                nameof(WorkflowStep.SoftwareSelection),
                LocalizationKeysConstants.SoftwareSelection.Text),
            ButtonRows = rows
        });
    }

    private List<WorkflowButtonRow> BuildNavigationRows()
    {
        return
        [
            new(new WorkflowButton(
                _localizer.GetPhrase(
                    nameof(WorkflowStep.SoftwareSelection),
                    LocalizationKeysConstants.SoftwareSelection.ChooseAnotherRoom),
                CallbackConstants.RoomPagePrefix + PaginationLimits.FirstPage)),
            new(new WorkflowButton(
                _localizer.GetPhrase(
                    nameof(WorkflowStep.SoftwareSelection),
                    LocalizationKeysConstants.SoftwareSelection.ChooseAnotherCategory),
                CallbackConstants.CategoryPagePrefix + PaginationLimits.FirstPage))
        ];
    }
}
