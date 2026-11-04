using Microsoft.EntityFrameworkCore;
using Pandora.Infrastructure.Database;

namespace Pandora.Api.Extensions;

public static class ApplicationConfigurationExtensions
{
    public static async Task<WebApplication> AddApplicationConfiguration(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();

        await context.Database.MigrateAsync();

        return app;
    }
}
