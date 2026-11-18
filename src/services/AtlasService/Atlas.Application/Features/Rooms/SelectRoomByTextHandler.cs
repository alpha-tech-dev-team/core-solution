using Atlas.Domain.Constants;
using Atlas.Domain.Constants.Localization;
using Atlas.Domain.Enums;
using Atlas.Domain.Interfaces.Repositories;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Rooms;

public sealed class SelectRoomByTextHandler : IRequestHandler<SelectRoomByTextMessage, Result<WorkflowResponse>>
{
    private readonly IRoomRepository _roomRepository;
    private readonly IMediator _mediator;
    private readonly ILocalizer _localizer;

    public SelectRoomByTextHandler(IRoomRepository roomRepository, IMediator mediator, ILocalizer localizer)
    {
        _roomRepository = roomRepository;
        _mediator = mediator;
        _localizer = localizer;
    }

    public async Task<Result<WorkflowResponse>> Handle(SelectRoomByTextMessage request, CancellationToken cancellationToken)
    {
        var rooms = await _roomRepository.FindByNameAsync(request.Text, RoomSorting.Apply, cancellationToken);

        var requested = request.Text.Trim();

        var room = rooms.FirstOrDefault(candidate => string.Equals(candidate.Name, requested, StringComparison.OrdinalIgnoreCase))
            ?? (rooms is [var singleRoom] ? singleRoom : null);

        if (room is not null)
        {
            return await _mediator.Send(
                new SelectRoomMessage(request.TelegramUserId, request.MessageId, room.Id),
                cancellationToken);
        }

        if (request.RoomMenuMessageId is not int menuMessageId)
        {
            return Result<WorkflowResponse>.Failure(nameof(ErrorCode.RoomNotFound));
        }

        var menu = await _mediator.Send(
            new ShowRoomsMessage(request.TelegramUserId, menuMessageId, PaginationLimits.FirstPage),
            cancellationToken);

        if (!menu.IsSuccess)
        {
            return menu;
        }

        return Result<WorkflowResponse>.Success(new WorkflowResponse
        {
            Text = _localizer.GetPhrase(
                nameof(WorkflowStep.RoomSelection),
                LocalizationKeysConstants.RoomSelection.RoomNotFound),
            ButtonRows = menu.Value!.ButtonRows
        });
    }
}
