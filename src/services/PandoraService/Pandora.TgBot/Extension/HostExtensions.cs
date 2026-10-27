using Microsoft.EntityFrameworkCore;
using Pandora.Infrastructure.Database;

namespace Pandora.TgBot.Extension;

public static class HostExtensions
{
    public static async Task ApplyMigrations(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<PandoraDbContext>().Database.MigrateAsync();
    }
}
