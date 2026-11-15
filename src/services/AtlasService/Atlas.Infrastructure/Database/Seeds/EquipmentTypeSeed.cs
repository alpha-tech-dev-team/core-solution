using Atlas.Domain.Constants;
using Atlas.Domain.Entities;
using Atlas.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Database.Seeds;

public static class EquipmentTypeSeed
{
    public static IReadOnlyList<EquipmentType> Types { get; } =
    [
        new EquipmentType
        {
            Id = EquipmentTypeConstants.Computer.Id,
            Name = EquipmentTypeConstants.Computer.Name,
            Description = EquipmentTypeConstants.Computer.Description
        },
        new EquipmentType
        {
            Id = EquipmentTypeConstants.Printer.Id,
            Name = EquipmentTypeConstants.Printer.Name,
            Description = EquipmentTypeConstants.Printer.Description
        },
        new EquipmentType
        {
            Id = EquipmentTypeConstants.MultifunctionDevice.Id,
            Name = EquipmentTypeConstants.MultifunctionDevice.Name,
            Description = EquipmentTypeConstants.MultifunctionDevice.Description
        },
        new EquipmentType
        {
            Id = EquipmentTypeConstants.Monitor.Id,
            Name = EquipmentTypeConstants.Monitor.Name,
            Description = EquipmentTypeConstants.Monitor.Description
        }
    ];

    public static async Task SeedAsync(AtlasDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.EquipmentTypes.ToDictionaryAsync(
            type => type.Id,
            cancellationToken);

        var changed = Types
            .Where(type => !existing.TryGetValue(type.Id, out var current)
                || current.Name != type.Name
                || current.Description != type.Description)
            .ToList();

        if (changed.Count == 0)
        {
            return;
        }

        foreach (var type in changed)
        {
            if (existing.TryGetValue(type.Id, out var current))
            {
                dbContext.Entry(current).CurrentValues.SetValues(type);
            }
            else
            {
                dbContext.EquipmentTypes.Add(type);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
