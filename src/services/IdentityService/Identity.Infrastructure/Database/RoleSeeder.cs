using Identity.Domain.Entities;
using RoleType = CoreSolution.Shared.Enums.UserRole;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Database;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        var existingRoles = await context.Roles
            .Select(role => role.Name)
            .ToListAsync(cancellationToken);

        var missingRoles = Enum.GetValues<RoleType>()
            .Select(role => role.ToString())
            .Where(name => !existingRoles.Contains(name))
            .Select(name => new Roles { Id = Guid.NewGuid(), Name = name })
            .ToArray();

        if (missingRoles.Length == 0)
        {
            return;
        }

        context.Roles.AddRange(missingRoles);
        await context.SaveChangesAsync(cancellationToken);
    }
}
