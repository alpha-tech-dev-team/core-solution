using Atlas.Domain.Constants;
using Atlas.Domain.Entities;
using Atlas.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Database.Seeds;

public static class SoftwareSeed
{
    public static IReadOnlyList<Software> Items { get; } =
    [
        new Software
        {
            Id = SoftwareConstants.Accounting.Id,
            Name = SoftwareConstants.Accounting.Name,
            Version = SoftwareConstants.Accounting.Version
        },
        new Software
        {
            Id = SoftwareConstants.OfficeSuite.Id,
            Name = SoftwareConstants.OfficeSuite.Name,
            Version = SoftwareConstants.OfficeSuite.Version
        }
    ];

    public static async Task SeedAsync(AtlasDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.Software.ToDictionaryAsync(
            item => item.Id,
            cancellationToken);

        var changed = Items
            .Where(item => !existing.TryGetValue(item.Id, out var current)
                || current.Name != item.Name
                || current.Version != item.Version)
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
                dbContext.Software.Add(item);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
