using Microsoft.EntityFrameworkCore;
using Pandora.Application.Services;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Interfaces.Services;
using Pandora.Domain.Options;
using Pandora.Infrastructure.Database;
using Pandora.Infrastructure.Repositories;
using Serilog;

namespace Pandora.Api.Extensions;

public static class ServiceConfigurationExtensions
{
    public static IServiceCollection AddServiceConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi();

        services.AddSerilog((serviceProvider, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(serviceProvider));

        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {DatabaseOptions.SectionName}.");

        ArgumentException.ThrowIfNullOrWhiteSpace(databaseOptions.ConnectionString);

        services.AddDbContext<PandoraDbContext>(options => options.UseNpgsql(databaseOptions.ConnectionString));

        services.AddScoped<ISecretMetadataRepository, SecretMetadataRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();

        services.AddSecretWriter(configuration);

        services.AddScoped<ISecretAdminService, SecretAdminService>();

        services.AddJwtAuthentication(configuration);

        return services;
    }
}
