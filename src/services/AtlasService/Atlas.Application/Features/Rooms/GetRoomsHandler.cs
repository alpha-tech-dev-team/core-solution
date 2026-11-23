using Atlas.Domain.Interfaces.Repositories;
using Atlas.Domain.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Rooms;

public sealed class GetRoomsHandler : IRequestHandler<GetRoomsMessage, Result<RoomListPage>>
{
    private readonly IRoomRepository _roomRepository;

    public GetRoomsHandler(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<Result<RoomListPage>> Handle(
        GetRoomsMessage request,
        CancellationToken cancellationToken)
    {
        var page = await _roomRepository.GetListPageAsync(
            new RoomPageQuery(request, RoomSorting.Apply),
            cancellationToken);

        return Result<RoomListPage>.Success(page);
    }
}
