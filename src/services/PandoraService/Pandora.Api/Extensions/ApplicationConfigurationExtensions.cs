using Microsoft.EntityFrameworkCore;
using Pandora.Infrastructure.Database;

namespace Pandora.Api.Extensions;

public static class ApplicationConfigurationExtensions
{
    public static async Task ApplyMigrations(this WebApplicationBuilder builder)
    {
        builder.Services.AddDatabaseConfiguration(builder.Configuration);

        await using var app = builder.Build();

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();

        await context.Database.MigrateAsync();
    }
}
