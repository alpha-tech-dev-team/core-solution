using Atlas.Domain.Options;
using Atlas.Infrastructure.Database.Context;
using Atlas.Infrastructure.Database.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pgvector.EntityFrameworkCore;

namespace Atlas.Api.Extensions.Configuration;

public static class DatabaseConfigurationExtensions
{
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services)
    {
        services.AddDbContext<AtlasDbContext>((serviceProvider, options) =>
        {
            var connectionString = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString;

            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.UseVector();
                npgsql.MapAtlasEnums();
            });
        });

        services.AddRepositoriesConfiguration();

        return services;
    }
}
