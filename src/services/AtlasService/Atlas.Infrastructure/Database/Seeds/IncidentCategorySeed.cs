using Atlas.Infrastructure.Database.Context;
using Atlas.Domain.Constants;
using Atlas.Domain.Entities;
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
        var existingNames = await dbContext.IncidentCategories
            .Select(category => category.Name)
            .ToListAsync(cancellationToken);

        var missing = Categories
            .Where(category => !existingNames.Contains(category.Name, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (missing.Count == 0)
        {
            return;
        }

        dbContext.IncidentCategories.AddRange(missing);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
