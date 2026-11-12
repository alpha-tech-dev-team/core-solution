using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using RoleType = CoreSolution.Shared.Enums.UserRole;

namespace Identity.Infrastructure.Database;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        var existingRoles = await context.Roles
            .Select(role => role.Name)
            .ToListAsync(cancellationToken);

        var knownRoles = Enum.GetValues<RoleType>()
            .Select(role => role.ToString())
            .ToArray();

        var unknownRoles = existingRoles.Except(knownRoles, StringComparer.OrdinalIgnoreCase).ToArray();
        if (unknownRoles.Length > 0)
        {
            throw new InvalidOperationException(
                $"Таблица ролей содержит значения, отсутствующие в enum {nameof(RoleType)}: {string.Join(", ", unknownRoles)}. "
                + "Названия ролей в базе должны совпадать с enum.");
        }

        var missingRoles = knownRoles
            .Where(name => !existingRoles.Contains(name, StringComparer.OrdinalIgnoreCase))
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
