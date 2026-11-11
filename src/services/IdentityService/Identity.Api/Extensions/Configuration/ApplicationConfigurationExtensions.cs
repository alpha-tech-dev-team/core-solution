using Identity.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Extensions.Configuration;

public static class ApplicationConfigurationExtensions
{
    public static async Task<WebApplication> AddApplicationConfiguration(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync();
        await RoleSeeder.SeedRolesAsync(context);

        return app;
    }
}
