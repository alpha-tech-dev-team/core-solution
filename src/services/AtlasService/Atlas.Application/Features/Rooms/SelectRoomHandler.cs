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

namespace Atlas.Application.Features.Rooms;

public sealed class SelectRoomHandler : IRequestHandler<SelectRoomMessage, Result<WorkflowResponse>>
{
    private readonly IRoomRepository _roomRepository;
    private readonly IRequestContext _requestContext;
    private readonly ISessionService _sessionService;
    private readonly ILocalizer _localizer;

    public SelectRoomHandler(
        IRoomRepository roomRepository,
        IRequestContext requestContext,
        ISessionService sessionService,
        ILocalizer localizer)
    {
        _roomRepository = roomRepository;
        _requestContext = requestContext;
        _sessionService = sessionService;
        _localizer = localizer;
    }

    public async Task<Result<WorkflowResponse>> Handle(SelectRoomMessage request, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetByIdAsync(request.RoomId, cancellationToken);

        if (room is null)
        {
            return Result<WorkflowResponse>.Failure(nameof(ErrorCode.RoomNotFound));
        }

        var session = (AtlasUserSession)_requestContext.Session;
        session.RoomId = room.Id;

        await _sessionService.UpdateState(session, nameof(WorkflowStep.RoomSelected), cancellationToken);

        return Result<WorkflowResponse>.Success(new WorkflowResponse
        {
            Text = string.Format(
                _localizer.GetPhrase(
                    nameof(WorkflowStep.RoomSelected),
                    LocalizationKeysConstants.RoomSelected.Text),
                room.Name),
            ButtonRows = new List<WorkflowButtonRow>
            {
                new(new WorkflowButton(
                    _localizer.GetPhrase(
                        nameof(WorkflowStep.RoomSelected),
                        LocalizationKeysConstants.RoomSelected.Button),
                    CallbackConstants.RoomPagePrefix + PaginationLimits.FirstPage))
            }
        });
    }
}
