using Atlas.Domain.Constants;
using Atlas.Domain.Entities;
using Atlas.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Database.Seeds;

public static class RoomSeed
{
    public static IReadOnlyList<Room> Rooms { get; } =
    [
        new Room
        {
            Id = RoomConstants.Office204.Id,
            Name = RoomConstants.Office204.Name,
            Description = RoomConstants.Office204.Description
        },
        new Room
        {
            Id = RoomConstants.Office305.Id,
            Name = RoomConstants.Office305.Name,
            Description = RoomConstants.Office305.Description
        },
        new Room
        {
            Id = RoomConstants.Office101.Id,
            Name = RoomConstants.Office101.Name,
            Description = RoomConstants.Office101.Description
        }
    ];

    public static async Task SeedAsync(AtlasDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.Rooms.ToDictionaryAsync(room => room.Id, cancellationToken);

        var changed = Rooms
            .Where(room => !existing.TryGetValue(room.Id, out var current)
                || current.Name != room.Name
                || current.Description != room.Description)
            .ToList();

        if (changed.Count == 0)
        {
            return;
        }

        foreach (var room in changed)
        {
            if (existing.TryGetValue(room.Id, out var current))
            {
                dbContext.Entry(current).CurrentValues.SetValues(room);
            }
            else
            {
                dbContext.Rooms.Add(room);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
