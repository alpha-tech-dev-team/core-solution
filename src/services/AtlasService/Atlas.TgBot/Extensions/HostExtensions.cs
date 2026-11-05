using Atlas.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Atlas.TgBot.Extensions;

public static class HostExtensions
{
    public static async Task ApplyMigrations(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AtlasDbContext>().Database.MigrateAsync();
    }
}
