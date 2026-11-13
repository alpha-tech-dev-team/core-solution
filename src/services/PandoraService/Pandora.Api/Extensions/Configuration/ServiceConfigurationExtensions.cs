using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pandora.Api.Middleware;
using Pandora.Application.Features.Secrets.Common;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Options;
using Pandora.Infrastructure.Database;
using Pandora.Infrastructure.Repositories;
using Pandora.Infrastructure.Secrets.Synchronization;
using Serilog;
using ApplicationAssemblyMarker = Pandora.Application.AssemblyMarker;

namespace Pandora.Api.Extensions.Configuration;

public static class ServiceConfigurationExtensions
{
    public static IServiceCollection AddServiceConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi();

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddSerilog((serviceProvider, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(serviceProvider));

        services.AddOptionsConfiguration(configuration);

        services.AddDatabaseConfiguration();

        services.AddSecretWriter();

        services.AddScoped<SecretAuditLogger>();

        services.AddScoped<SecretSyncService>();

        services.AddHostedService<SecretSyncWorker>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<ApplicationAssemblyMarker>());

        services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>();

        services.AddJwtAuthentication();

        return services;
    }

    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services)
    {
        services.AddScoped<IAuditContext, AuditContext>();
        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<PandoraDbContext>((serviceProvider, options) => options
            .UseNpgsql(serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString)
            .AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>()));

        services.AddScoped<ISecretMetadataRepository, SecretMetadataRepository>();

        return services;
    }
}
