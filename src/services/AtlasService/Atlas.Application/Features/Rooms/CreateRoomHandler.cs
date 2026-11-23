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
        var name = request.Name.Trim();
        var description = NormalizeDescription(request.Description);

        if (await _roomRepository.ExistsByNameAsync(name, cancellationToken: cancellationToken))
        {
            return Result<RoomPageItem>.Failure(nameof(ErrorCode.Conflict));
        }

        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        _roomRepository.Add(room);
        await _roomRepository.SaveChangesAsync(cancellationToken);

        return Result<RoomPageItem>.Success(
            new RoomPageItem(room.Id, room.Name, room.Description, room.CreatedAt));
    }

    private static string? NormalizeDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }
}
