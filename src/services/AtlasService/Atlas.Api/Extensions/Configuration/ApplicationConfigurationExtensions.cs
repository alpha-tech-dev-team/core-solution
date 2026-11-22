using Atlas.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Api.Extensions.Configuration;

public static class ApplicationConfigurationExtensions
{
    public static async Task ApplyMigrations(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptionsConfiguration(builder.Configuration);
        builder.Services.AddDatabaseConfiguration();

        await using var app = builder.Build();

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AtlasDbContext>();

        await context.Database.MigrateAsync();
    }
}
