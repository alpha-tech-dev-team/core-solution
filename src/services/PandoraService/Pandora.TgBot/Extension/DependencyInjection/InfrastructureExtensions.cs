using CoreSolution.Shared.TgBot.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Options;
using Pandora.Infrastructure.Audit;
using Pandora.Infrastructure.Database;
using Pandora.Infrastructure.Repositories;
using Pandora.Infrastructure.Secrets;

namespace Pandora.TgBot.Extension.DependencyInjection;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddBotInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {DatabaseOptions.SectionName}.");

        ArgumentException.ThrowIfNullOrWhiteSpace(databaseOptions.ConnectionString);

        services.AddIdentityClient(configuration);
        services.AddLocalization(typeof(ConfigSecretStore).Assembly);
        services.AddDbContext<PandoraDbContext>((serviceProvider, options) => options
            .UseNpgsql(databaseOptions.ConnectionString)
            .AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>()));
        services.AddScoped<IAuditContext, AuditContext>();
        services.AddScoped<IAuditSaver, AuditSaver>();
        services.AddScoped<AuditSaveChangesInterceptor>(serviceProvider =>
            new AuditSaveChangesInterceptor(
                serviceProvider.GetRequiredService<IAuditContext>(),
                !string.IsNullOrWhiteSpace(databaseOptions.RedisConnection)));
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddSessionInfrastructure(configuration, databaseOptions.RedisConnection);

        if (!string.IsNullOrWhiteSpace(databaseOptions.RedisConnection))
        {
            services.AddSingleton<IAuditNotifier, RedisAuditNotifier>();
            services.AddHostedService<AuditOutboxPublisher>();
        }

        return services;
    }
}
