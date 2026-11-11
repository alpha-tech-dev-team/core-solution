using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Pandora.Api.Middleware;
using Pandora.Application.Features.Secrets.Common;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Options;
using Pandora.Infrastructure.Database;
using Pandora.Infrastructure.Repositories;
using Serilog;
using ApplicationAssemblyMarker = Pandora.Application.AssemblyMarker;

namespace Pandora.Api.Extensions;

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

        services.AddDatabaseConfiguration(configuration);

        services.AddSecretWriter(configuration);

        services.AddScoped<SecretAuditLogger>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<ApplicationAssemblyMarker>());

        services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>();

        services.AddJwtAuthentication(configuration);

        return services;
    }

    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {DatabaseOptions.SectionName}.");

        ArgumentException.ThrowIfNullOrWhiteSpace(databaseOptions.ConnectionString);

        services.AddScoped<IAuditContext, AuditContext>();
        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<PandoraDbContext>((serviceProvider, options) => options
            .UseNpgsql(databaseOptions.ConnectionString)
            .AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>()));

        services.AddScoped<ISecretMetadataRepository, SecretMetadataRepository>();

        return services;
    }
}
