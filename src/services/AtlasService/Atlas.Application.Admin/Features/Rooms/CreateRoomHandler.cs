using Atlas.Domain.Entities;
using Atlas.Domain.Enums;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Domain.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Rooms;

public sealed class CreateRoomHandler : IRequestHandler<CreateRoomMessage, Result<RoomPageItem>>
{
    private readonly IRoomRepository _roomRepository;

    public CreateRoomHandler(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<Result<RoomPageItem>> Handle(
        CreateRoomMessage request,
        CancellationToken cancellationToken)
    {
        var room = new Room(request.Name, request.Description);

        if (await _roomRepository.ExistsByNameAsync(room.Name, cancellationToken: cancellationToken))
        {
            return Result<RoomPageItem>.Failure(nameof(ErrorCode.Conflict));
        }

        _roomRepository.Add(room);
        await _roomRepository.SaveChangesAsync(cancellationToken);

        return Result<RoomPageItem>.Success(
            new RoomPageItem(room.Id, room.Name, room.Description, room.CreatedAt));
    }
}
