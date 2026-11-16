using Atlas.Domain.Entities;

namespace Atlas.Application.Features.Rooms;

public static class RoomSorting
{
    public const string NameField = nameof(Room.Name);
    public const string IdField = nameof(Room.Id);
    public const string DefaultField = NameField;

    public static IOrderedQueryable<Room> Apply(IQueryable<Room> query)
    {
        return Apply(query, DefaultField);
    }

    public static IOrderedQueryable<Room> Apply(IQueryable<Room> query, string sortField)
    {
        var ordered = sortField switch
        {
            IdField => query.OrderBy(room => room.Id),
            _ => query.OrderBy(room => room.Name)
        };

        return ordered.ThenBy(room => room.Id);
    }
}
