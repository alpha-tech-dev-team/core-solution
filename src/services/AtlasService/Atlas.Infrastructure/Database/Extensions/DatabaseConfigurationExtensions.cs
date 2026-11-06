using Atlas.Domain.Constants.AtlasDBConfiguration;
using Atlas.Domain.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pgvector.EntityFrameworkCore;

namespace Atlas.Infrastructure.Database.Extensions;

public static class DatabaseConfigurationExtensions
{
    public static IServiceCollection AddAtlasDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration[$"{DatabaseOptions.SectionName}:{nameof(DatabaseOptions.ConnectionString)}"];

        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<AtlasDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.UseVector();
                npgsql.MigrationsHistoryTable(DBKeys.Shared.MigrationsHistoryTable, DBKeys.Shared.Schema);
            }));

        return services;
    }
}
