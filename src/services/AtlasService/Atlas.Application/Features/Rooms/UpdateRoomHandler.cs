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
        var room = await _roomRepository.GetByIdAsync(request.RoomId, cancellationToken);

        if (room is null)
        {
            return Result<RoomPageItem>.Failure(nameof(ErrorCode.RoomNotFound));
        }

        var name = request.Name.Trim();
        var description = NormalizeDescription(request.Description);

        if (await _roomRepository.ExistsByNameAsync(name, room.Id, cancellationToken))
        {
            return Result<RoomPageItem>.Failure(nameof(ErrorCode.Conflict));
        }

        room.Name = name;
        room.Description = description;

        _roomRepository.Update(room);
        await _roomRepository.SaveChangesAsync(cancellationToken);

        return Result<RoomPageItem>.Success(
            new RoomPageItem(room.Id, room.Name, room.Description, room.CreatedAt));
    }

    private static string? NormalizeDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }
}
