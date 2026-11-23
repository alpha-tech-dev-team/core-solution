using Atlas.Api.Middleware;
using Atlas.Application.Behaviors;
using Atlas.Application.Features.Incidents;
using Atlas.Application.Validators;
using FluentValidation;
using MediatR;
using Serilog;

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
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<GetIncidentsHandler>();
            cfg.TypeEvaluator = type => type == typeof(GetIncidentsHandler);
        });
        services.AddValidatorsFromAssemblyContaining<GetIncidentsMessageValidator>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddJwtAuthentication();

        return services;
    }
}
