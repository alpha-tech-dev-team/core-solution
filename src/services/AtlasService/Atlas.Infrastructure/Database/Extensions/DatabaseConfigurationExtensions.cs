using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pgvector.EntityFrameworkCore;

namespace Atlas.Infrastructure.Database.Extensions;

public static class DatabaseConfigurationExtensions
{
    public static IServiceCollection AddAtlasDatabase(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<AtlasDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.UseVector();
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "atlas");
            }));

        return services;
    }
}
