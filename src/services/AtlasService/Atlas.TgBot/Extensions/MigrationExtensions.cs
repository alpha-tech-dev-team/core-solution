using Atlas.Domain.Options;
using Atlas.Infrastructure.Database.Context;
using Atlas.Infrastructure.Database.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atlas.TgBot.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrations(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;

        var dbContext = services.GetRequiredService<AtlasDbContext>();
        var options = services.GetRequiredService<IOptions<DatabaseOptions>>().Value;

        await dbContext.Database.MigrateAsync();

        if (!options.SeedOnStartup)
        {
            return;
        }

        services.GetRequiredService<ILogger<MigrationLog>>()
            .LogInformation("Применяется сид категорий инцидентов.");

        await IncidentCategorySeed.SeedAsync(dbContext);

        services.GetRequiredService<ILogger<MigrationLog>>()
            .LogInformation("Применяется сид типов оборудования.");

        await EquipmentTypeSeed.SeedAsync(dbContext);

        services.GetRequiredService<ILogger<MigrationLog>>()
            .LogInformation("Применяется сид кабинетов.");

        await RoomSeed.SeedAsync(dbContext);
    }

    private sealed class MigrationLog
    {
    }
}
