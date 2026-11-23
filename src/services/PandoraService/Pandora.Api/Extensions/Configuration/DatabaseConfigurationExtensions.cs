using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Options;
using Pandora.Infrastructure.Database;

namespace Pandora.Api.Extensions.Configuration;

public static class DatabaseConfigurationExtensions
{
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services)
    {
        services.AddScoped<IAuditContext, AuditContext>();
        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<PandoraDbContext>((serviceProvider, options) => options
            .UseNpgsql(serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString)
            .AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>()));

        services.AddRepositoriesConfiguration();

        return services;
    }
}
