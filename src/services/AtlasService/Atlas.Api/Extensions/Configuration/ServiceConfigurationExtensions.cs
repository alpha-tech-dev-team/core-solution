using Atlas.Api.Middleware;
using FluentValidation;
using Serilog;
using ApplicationAssemblyMarker = Atlas.Application.AssemblyMarker;

namespace Atlas.Api.Extensions.Configuration;

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
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<ApplicationAssemblyMarker>());
        services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>();
        services.AddJwtAuthentication();

        return services;
    }
}
