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

namespace Atlas.Application.Features.Hardware;

public sealed class ShowHardwareHandler : IRequestHandler<ShowHardwareMessage, Result<WorkflowResponse>>
{
    private readonly IHardwareRepository _hardwareRepository;
    private readonly IIncidentCategoryRepository _categoryRepository;
    private readonly IRequestContext _requestContext;
    private readonly ISessionService _sessionService;
    private readonly ILocalizer _localizer;

    public ShowHardwareHandler(
        IHardwareRepository hardwareRepository,
        IIncidentCategoryRepository categoryRepository,
        IRequestContext requestContext,
        ISessionService sessionService,
        ILocalizer localizer)
    {
        _hardwareRepository = hardwareRepository;
        _categoryRepository = categoryRepository;
        _requestContext = requestContext;
        _sessionService = sessionService;
        _localizer = localizer;
    }

    public async Task<Result<WorkflowResponse>> Handle(ShowHardwareMessage request, CancellationToken cancellationToken)
    {
        var session = (AtlasUserSession)_requestContext.Session;

        if (session.RoomId is not Guid roomId || session.IncidentCategoryId is not Guid categoryId)
        {
            return Result<WorkflowResponse>.Failure(nameof(ErrorCode.IncidentContextIncomplete));
        }

        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);

        if (category?.Type != IncidentCategoryType.Hardware)
        {
            return Result<WorkflowResponse>.Failure(nameof(ErrorCode.IncidentContextIncomplete));
        }

        var page = await _hardwareRepository.GetPageByRoomAsync(
            roomId,
            new PageRequest(request.Page, PaginationLimits.DefaultPageSize),
            query => query.OrderBy(item => item.Name).ThenBy(item => item.Id),
            cancellationToken);

        if (page.Total == 0)
        {
            return Result<WorkflowResponse>.Success(new WorkflowResponse
            {
                Text = _localizer.GetPhrase(
                    nameof(WorkflowStep.HardwareSelection),
                    LocalizationKeysConstants.HardwareSelection.NotConfigured),
                ButtonRows = BuildFallbackRows()
            });
        }

        await _sessionService.UpdateState(session, nameof(WorkflowStep.HardwareSelection), cancellationToken);
        var rows = page.Items
            .Select(item => new WorkflowButtonRow(new WorkflowButton(
                item.Name,
                CallbackConstants.HardwarePrefix + item.Id)))
            .ToList();

        if (page.HasNextPage)
        {
            rows.Add(new WorkflowButtonRow(new WorkflowButton(
                _localizer.GetPhrase(
                    nameof(WorkflowStep.HardwareSelection),
                    LocalizationKeysConstants.HardwareSelection.NextPage),
                CallbackConstants.HardwarePagePrefix + page.NextPage)));
        }

        rows.AddRange(BuildNavigationRows());

        return Result<WorkflowResponse>.Success(new WorkflowResponse
        {
            Text = _localizer.GetPhrase(
                nameof(WorkflowStep.HardwareSelection),
                LocalizationKeysConstants.HardwareSelection.Text),
            ButtonRows = rows
        });
    }

    private List<WorkflowButtonRow> BuildFallbackRows()
    {
        return
        [
            new(new WorkflowButton(
                _localizer.GetPhrase(
                    nameof(WorkflowStep.HardwareSelection),
                    LocalizationKeysConstants.HardwareSelection.ChooseAnotherRoom),
                CallbackConstants.RoomPagePrefix + PaginationLimits.FirstPage)),
            new(new WorkflowButton(
                _localizer.GetPhrase(
                    nameof(WorkflowStep.HardwareSelection),
                    LocalizationKeysConstants.HardwareSelection.ChooseAnotherCategory),
                CallbackConstants.CategoryPagePrefix + PaginationLimits.FirstPage))
        ];
    }

    private List<WorkflowButtonRow> BuildNavigationRows()
    {
        return
        [
            new(new WorkflowButton(
                _localizer.GetPhrase(
                    nameof(WorkflowStep.HardwareSelection),
                    LocalizationKeysConstants.HardwareSelection.ChooseAnotherRoom),
                CallbackConstants.RoomPagePrefix + PaginationLimits.FirstPage)),
            new(new WorkflowButton(
                _localizer.GetPhrase(
                    nameof(WorkflowStep.HardwareSelection),
                    LocalizationKeysConstants.HardwareSelection.ChooseAnotherCategory),
                CallbackConstants.CategoryPagePrefix + PaginationLimits.FirstPage))
        ];
    }
}
