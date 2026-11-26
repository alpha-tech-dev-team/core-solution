using Atlas.Domain.Enums;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Domain.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Rooms;

public sealed class UpdateRoomHandler : IRequestHandler<UpdateRoomMessage, Result<RoomPageItem>>
{
    private readonly IRoomRepository _roomRepository;

    public UpdateRoomHandler(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<Result<RoomPageItem>> Handle(
        UpdateRoomMessage request,
        CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetByIdAsync(request.ItemId, cancellationToken);

        if (room is null)
        {
            return Result<RoomPageItem>.Failure(nameof(ErrorCode.RoomNotFound));
        }

        room.UpdateDetails(request.Name, request.Description);

        if (await _roomRepository.ExistsByNameAsync(room.Name, room.Id, cancellationToken))
        {
            return Result<RoomPageItem>.Failure(nameof(ErrorCode.Conflict));
        }

        _roomRepository.Update(room);
        await _roomRepository.SaveChangesAsync(cancellationToken);

        return Result<RoomPageItem>.Success(
            new RoomPageItem(room.Id, room.Name, room.Description, room.CreatedAt));
    }
}
