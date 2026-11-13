using Atlas.Domain.Constants;
using Atlas.Domain.Entities;
using Atlas.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Database.Seeds;

public static class IncidentCategorySeed
{
    public static IReadOnlyList<IncidentCategory> Categories { get; } =
    [
        new IncidentCategory
        {
            Id = IncidentCategoryConstants.Hardware.Id,
            Name = IncidentCategoryConstants.Hardware.Name,
            Description = IncidentCategoryConstants.Hardware.Description
        },
        new IncidentCategory
        {
            Id = IncidentCategoryConstants.Software.Id,
            Name = IncidentCategoryConstants.Software.Name,
            Description = IncidentCategoryConstants.Software.Description
        },
        new IncidentCategory
        {
            Id = IncidentCategoryConstants.Other.Id,
            Name = IncidentCategoryConstants.Other.Name,
            Description = IncidentCategoryConstants.Other.Description
        }
    ];

    public static async Task SeedAsync(AtlasDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.IncidentCategories.ToDictionaryAsync(
            category => category.Id,
            cancellationToken);

        var changed = Categories
            .Where(category => !existing.TryGetValue(category.Id, out var current)
                || current.Name != category.Name
                || current.Description != category.Description)
            .ToList();

        if (changed.Count == 0)
        {
            return;
        }

        foreach (var category in changed)
        {
            if (existing.TryGetValue(category.Id, out var current))
            {
                dbContext.Entry(current).CurrentValues.SetValues(category);
            }
            else
            {
                dbContext.IncidentCategories.Add(category);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
