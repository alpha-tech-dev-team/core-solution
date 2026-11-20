using Atlas.Application.Common;
using Atlas.Domain.Constants.Localization;
using Atlas.Domain.Enums;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Domain.Models;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Hardware;

public sealed class SelectHardwareHandler
    : SessionStateHandler<SelectHardwareMessage>,
      IRequestHandler<SelectHardwareMessage, Result<WorkflowResponse>>
{
    private readonly IHardwareRepository _hardwareRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IIncidentCategoryRepository _categoryRepository;
    private readonly ILocalizer _localizer;

    protected override string NextWorkflowState => nameof(WorkflowStep.IncidentDescription);

    public SelectHardwareHandler(
        IHardwareRepository hardwareRepository,
        IRoomRepository roomRepository,
        IIncidentCategoryRepository categoryRepository,
        IRequestContext requestContext,
        ISessionService sessionService,
        ILocalizer localizer)
        : base(requestContext, sessionService)
    {
        _hardwareRepository = hardwareRepository;
        _roomRepository = roomRepository;
        _categoryRepository = categoryRepository;
        _localizer = localizer;
    }

    public async Task<Result<WorkflowResponse>> Handle(SelectHardwareMessage request, CancellationToken cancellationToken)
    {
        if (Session.RoomId is not Guid roomId || Session.IncidentCategoryId is not Guid categoryId)
        {
            return Result<WorkflowResponse>.Failure(nameof(ErrorCode.IncidentContextIncomplete));
        }

        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);

        if (category?.Type != IncidentCategoryType.Hardware)
        {
            return Result<WorkflowResponse>.Failure(nameof(ErrorCode.IncidentContextIncomplete));
        }

        var hardware = await _hardwareRepository.GetByIdInRoomAsync(request.HardwareId, roomId, cancellationToken);

        if (hardware is null)
        {
            return Result<WorkflowResponse>.Failure(nameof(ErrorCode.HardwareNotFound));
        }

        var room = await _roomRepository.GetByIdAsync(roomId, cancellationToken);

        await UpdateState(request, cancellationToken);

        return Result<WorkflowResponse>.Success(new WorkflowResponse
        {
            Text = string.Format(
                _localizer.GetPhrase(
                    nameof(WorkflowStep.IncidentDescription),
                    LocalizationKeysConstants.IncidentDescription.Text),
                room?.Name ?? string.Empty,
                hardware.Name)
        });
    }

    protected override void ApplyState(AtlasUserSession session, SelectHardwareMessage request)
    {
        session.HardwareId = request.HardwareId;
        session.SoftwareId = null;
    }
}
