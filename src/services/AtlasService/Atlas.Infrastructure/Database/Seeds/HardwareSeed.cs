using Atlas.Domain.Constants;
using Atlas.Domain.Entities;
using Atlas.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Database.Seeds;

public static class HardwareSeed
{
    public static IReadOnlyList<Hardware> Items { get; } =
    [
        new Hardware
        {
            Id = HardwareConstants.Office101Computer.Id,
            Name = HardwareConstants.Office101Computer.Name,
            EquipmentTypeId = EquipmentTypeConstants.Computer.Id,
            InventoryNumber = HardwareConstants.Office101Computer.InventoryNumber,
            SerialNumber = HardwareConstants.Office101Computer.SerialNumber,
            RoomId = RoomConstants.Office101.Id
        },
        new Hardware
        {
            Id = HardwareConstants.Office101Monitor.Id,
            Name = HardwareConstants.Office101Monitor.Name,
            EquipmentTypeId = EquipmentTypeConstants.Monitor.Id,
            InventoryNumber = HardwareConstants.Office101Monitor.InventoryNumber,
            SerialNumber = HardwareConstants.Office101Monitor.SerialNumber,
            RoomId = RoomConstants.Office101.Id
        },
        new Hardware
        {
            Id = HardwareConstants.Office204Computer.Id,
            Name = HardwareConstants.Office204Computer.Name,
            EquipmentTypeId = EquipmentTypeConstants.Computer.Id,
            InventoryNumber = HardwareConstants.Office204Computer.InventoryNumber,
            SerialNumber = HardwareConstants.Office204Computer.SerialNumber,
            RoomId = RoomConstants.Office204.Id
        },
        new Hardware
        {
            Id = HardwareConstants.Office204Printer.Id,
            Name = HardwareConstants.Office204Printer.Name,
            EquipmentTypeId = EquipmentTypeConstants.Printer.Id,
            InventoryNumber = HardwareConstants.Office204Printer.InventoryNumber,
            SerialNumber = HardwareConstants.Office204Printer.SerialNumber,
            RoomId = RoomConstants.Office204.Id
        },
        new Hardware
        {
            Id = HardwareConstants.Office305Computer.Id,
            Name = HardwareConstants.Office305Computer.Name,
            EquipmentTypeId = EquipmentTypeConstants.Computer.Id,
            InventoryNumber = HardwareConstants.Office305Computer.InventoryNumber,
            SerialNumber = HardwareConstants.Office305Computer.SerialNumber,
            RoomId = RoomConstants.Office305.Id
        },
        new Hardware
        {
            Id = HardwareConstants.Office305MultifunctionDevice.Id,
            Name = HardwareConstants.Office305MultifunctionDevice.Name,
            EquipmentTypeId = EquipmentTypeConstants.MultifunctionDevice.Id,
            InventoryNumber = HardwareConstants.Office305MultifunctionDevice.InventoryNumber,
            SerialNumber = HardwareConstants.Office305MultifunctionDevice.SerialNumber,
            RoomId = RoomConstants.Office305.Id
        },
        new Hardware
        {
            Id = HardwareConstants.Office401Computer.Id,
            Name = HardwareConstants.Office401Computer.Name,
            EquipmentTypeId = EquipmentTypeConstants.Computer.Id,
            InventoryNumber = HardwareConstants.Office401Computer.InventoryNumber,
            SerialNumber = HardwareConstants.Office401Computer.SerialNumber,
            RoomId = RoomConstants.Office401.Id
        },
        new Hardware
        {
            Id = HardwareConstants.Office401Printer.Id,
            Name = HardwareConstants.Office401Printer.Name,
            EquipmentTypeId = EquipmentTypeConstants.Printer.Id,
            InventoryNumber = HardwareConstants.Office401Printer.InventoryNumber,
            SerialNumber = HardwareConstants.Office401Printer.SerialNumber,
            RoomId = RoomConstants.Office401.Id
        },
        new Hardware
        {
            Id = HardwareConstants.Office402Computer.Id,
            Name = HardwareConstants.Office402Computer.Name,
            EquipmentTypeId = EquipmentTypeConstants.Computer.Id,
            InventoryNumber = HardwareConstants.Office402Computer.InventoryNumber,
            SerialNumber = HardwareConstants.Office402Computer.SerialNumber,
            RoomId = RoomConstants.Office402.Id
        },
        new Hardware
        {
            Id = HardwareConstants.Office507Computer.Id,
            Name = HardwareConstants.Office507Computer.Name,
            EquipmentTypeId = EquipmentTypeConstants.Computer.Id,
            InventoryNumber = HardwareConstants.Office507Computer.InventoryNumber,
            SerialNumber = HardwareConstants.Office507Computer.SerialNumber,
            RoomId = RoomConstants.Office507.Id
        },
        new Hardware
        {
            Id = HardwareConstants.Office508Computer.Id,
            Name = HardwareConstants.Office508Computer.Name,
            EquipmentTypeId = EquipmentTypeConstants.Computer.Id,
            InventoryNumber = HardwareConstants.Office508Computer.InventoryNumber,
            SerialNumber = HardwareConstants.Office508Computer.SerialNumber,
            RoomId = RoomConstants.Office508.Id
        }
    ];

    public static async Task SeedAsync(AtlasDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.Hardware.ToDictionaryAsync(
            item => item.Id,
            cancellationToken);

        var changed = Items
            .Where(item => !existing.TryGetValue(item.Id, out var current)
                || current.Name != item.Name
                || current.EquipmentTypeId != item.EquipmentTypeId
                || current.InventoryNumber != item.InventoryNumber
                || current.SerialNumber != item.SerialNumber
                || current.RoomId != item.RoomId)
            .ToList();

        if (changed.Count == 0)
        {
            return;
        }

        foreach (var item in changed)
        {
            if (existing.TryGetValue(item.Id, out var current))
            {
                dbContext.Entry(current).CurrentValues.SetValues(item);
            }
            else
            {
                dbContext.Hardware.Add(item);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
