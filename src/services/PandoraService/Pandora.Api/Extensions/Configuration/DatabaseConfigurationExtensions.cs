using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pandora.Api.Hubs;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Options;
using Pandora.Infrastructure.Audit;
using Pandora.Infrastructure.Database;
using StackExchange.Redis;

namespace Pandora.Api.Extensions.Configuration;

public static class DatabaseConfigurationExtensions
{
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()?.RedisConnection;
        var writeOutbox = !string.IsNullOrWhiteSpace(redisConnection);

        services.AddScoped<IAuditContext, AuditContext>();
        services.AddScoped<AuditSaveChangesInterceptor>(serviceProvider =>
            new AuditSaveChangesInterceptor(serviceProvider.GetRequiredService<IAuditContext>(), writeOutbox));

        services.AddDbContext<PandoraDbContext>((serviceProvider, options) => options
            .UseNpgsql(serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString)
            .AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>()));

        services.AddRepositoriesConfiguration();
        services.AddSignalR();

        if (writeOutbox)
        {
            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection!));
            services.AddSingleton<IAuditNotifier, RedisAuditNotifier>();
            services.AddHostedService<AuditNotificationSubscriber>();
            services.AddHostedService<AuditOutboxPublisher>();
        }

        return services;
    }
}
