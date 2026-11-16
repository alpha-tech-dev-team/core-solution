using Atlas.Domain.Entities;

namespace Atlas.Application.Features.Rooms;

public static class RoomSorting
{
    public const string DefaultField = nameof(Room.Name);

    public static IOrderedQueryable<Room> Apply(IQueryable<Room> query)
    {
        return query
            .OrderBy(room => room.Name)
            .ThenBy(room => room.Id);
    }
}
