using Atlas.Application.Common;
using Atlas.Application.Features.Hardware;
using Atlas.Application.Features.Software;
using Atlas.Domain.Constants;
using Atlas.Domain.Constants.Localization;
using Atlas.Domain.Enums;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Domain.Models;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Rooms;

public sealed class SelectRoomHandler
    : SessionStateHandler<SelectRoomMessage>, IRequestHandler<SelectRoomMessage, Result<WorkflowResponse>>
{
    private readonly IRoomRepository _roomRepository;
    private readonly IIncidentCategoryRepository _incidentCategoryRepository;
    private readonly ILocalizer _localizer;
    private readonly IMediator _mediator;

    protected override string NextWorkflowState => nameof(WorkflowStep.RoomSelection);

    public SelectRoomHandler(
        IRoomRepository roomRepository,
        IIncidentCategoryRepository incidentCategoryRepository,
        IRequestContext requestContext,
        ISessionService sessionService,
        ILocalizer localizer,
        IMediator mediator)
        : base(requestContext, sessionService)
    {
        _roomRepository = roomRepository;
        _incidentCategoryRepository = incidentCategoryRepository;
        _localizer = localizer;
        _mediator = mediator;
    }

    public async Task<Result<WorkflowResponse>> Handle(SelectRoomMessage request, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetByIdAsync(request.RoomId, cancellationToken);

        if (room is null)
        {
            return Result<WorkflowResponse>.Failure(nameof(ErrorCode.RoomNotFound));
        }

        await UpdateState(request, cancellationToken);

        if (Session.IncidentCategoryId is not Guid categoryId)
        {
            return Result<WorkflowResponse>.Failure(nameof(ErrorCode.IncidentContextIncomplete));
        }

        var category = await _incidentCategoryRepository.GetByIdAsync(categoryId, cancellationToken);

        if (category is null)
        {
            return Result<WorkflowResponse>.Failure(nameof(ErrorCode.IncidentCategoryNotFound));
        }

        return category.Type switch
        {
            IncidentCategoryType.Hardware => await _mediator.Send(
                new ShowHardwareMessage(request.TelegramUserId, request.MessageId, PaginationLimits.FirstPage),
                cancellationToken),
            IncidentCategoryType.Software => await _mediator.Send(
                new ShowSoftwareMessage(request.TelegramUserId, request.MessageId, PaginationLimits.FirstPage),
                cancellationToken),
            _ => await ShowDescriptionPromptAsync(room.Name, cancellationToken)
        };
    }

    private async Task<Result<WorkflowResponse>> ShowDescriptionPromptAsync(
        string roomName,
        CancellationToken cancellationToken)
    {
        await SessionService.UpdateState(Session, nameof(WorkflowStep.IncidentDescription), cancellationToken);

        return Result<WorkflowResponse>.Success(new WorkflowResponse
        {
            Text = string.Format(
                _localizer.GetPhrase(
                    nameof(WorkflowStep.IncidentDescription),
                    LocalizationKeysConstants.IncidentDescription.TextWithoutAsset),
                roomName),
            ButtonRows = new List<WorkflowButtonRow>
            {
                new(new WorkflowButton(
                    _localizer.GetPhrase(
                        nameof(WorkflowStep.IncidentDescription),
                        LocalizationKeysConstants.IncidentDescription.ChooseAnotherRoom),
                    CallbackConstants.RoomPagePrefix + PaginationLimits.FirstPage))
            }
        });
    }

    protected override void ApplyState(AtlasUserSession session, SelectRoomMessage request)
    {
        if (session.RoomId != request.RoomId)
        {
            session.HardwareId = null;
            session.SoftwareId = null;
        }

        session.RoomId = request.RoomId;
    }
}
