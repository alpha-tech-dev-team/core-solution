using Microsoft.EntityFrameworkCore;
using Pandora.Infrastructure.Database;

namespace Pandora.TgBot.Extension;

public static class WebApplicationExtensions
{
    public static async Task ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<PandoraDbContext>().Database.MigrateAsync();
    }
}
