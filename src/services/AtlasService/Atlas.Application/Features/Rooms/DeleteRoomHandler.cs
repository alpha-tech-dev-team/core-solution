using Atlas.Domain.Enums;
using Atlas.Domain.Interfaces.Repositories;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Rooms;

public sealed class DeleteRoomHandler : IRequestHandler<DeleteRoomMessage, Result>
{
    private readonly IRoomRepository _roomRepository;

    public DeleteRoomHandler(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<Result> Handle(
        DeleteRoomMessage request,
        CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetByIdAsync(request.RoomId, cancellationToken);

        if (room is null)
        {
            return Result.Failure(nameof(ErrorCode.RoomNotFound));
        }

        if (await _roomRepository.HasDependenciesAsync(request.RoomId, cancellationToken))
        {
            return Result.Failure(nameof(ErrorCode.Conflict));
        }

        _roomRepository.Remove(room);
        await _roomRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
