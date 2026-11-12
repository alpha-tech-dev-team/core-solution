using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Pandora.Api.Middleware;
using Pandora.Application.Features.Secrets.Common;
using Pandora.Domain.Constants;
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

        services.AddDatabaseConfiguration(configuration);

        services.AddSecretWriter(configuration);

        services.AddScoped<SecretAuditLogger>();

        services.AddOptions<SecretSyncOptions>()
            .Bind(configuration.GetSection(SecretSyncOptions.SectionName))
            .Validate(
                options => options.IntervalSeconds >= SecretSyncLimits.MinIntervalSeconds,
                $"IntervalSeconds must be at least {SecretSyncLimits.MinIntervalSeconds}")
            .Validate(
                options => options.BatchSize is >= SecretSyncLimits.MinBatchSize and <= SecretSyncLimits.MaxBatchSize,
                $"BatchSize must be between {SecretSyncLimits.MinBatchSize} and {SecretSyncLimits.MaxBatchSize}")
            .Validate(
                options => options.MaxAttempts is >= SecretSyncLimits.MinAttempts and <= SecretSyncLimits.MaxAttemptsLimit,
                $"MaxAttempts must be between {SecretSyncLimits.MinAttempts} and {SecretSyncLimits.MaxAttemptsLimit}")
            .Validate(
                options => options.MaxParallelism is >= SecretSyncLimits.MinParallelism and <= SecretSyncLimits.MaxParallelismLimit,
                $"MaxParallelism must be between {SecretSyncLimits.MinParallelism} and {SecretSyncLimits.MaxParallelismLimit}")
            .Validate(
                options => options.MaxRetryDelaySeconds > 0,
                "MaxRetryDelaySeconds must be positive")
            .ValidateOnStart();
        services.AddHostedService<SecretSyncWorker>();

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
