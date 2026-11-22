using Atlas.Api.Middleware;
using Atlas.Application.Features.Incidents;
using Atlas.Application.Validators;
using Atlas.Domain.Models;
using Atlas.Application.Behaviors;
using CoreSolution.Shared.Results;
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
        services.AddTransient<IRequestHandler<GetIncidentsMessage, Result<IncidentListPage>>, GetIncidentsHandler>();
        services.AddValidatorsFromAssemblyContaining<GetIncidentsMessageValidator>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddJwtAuthentication();

        return services;
    }
}
