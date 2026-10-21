using CoreSolution.Shared.Extensions;
using FluentValidation;
using Identity.App.Contracts;
using Identity.App.Services;
using Identity.App.Validators;
using Identity.Core.Interfaces.Repositories;
using Identity.Core.Interfaces.Services;
using Identity.Infrastructure.Database.Extensions;
using Identity.Infrastructure.Options.Extensions;
using Identity.Infrastructure.Repositories;
using Serilog;

namespace Identity.Api.Extensions;

public static class ServiceConfigurationExtensions
{
    public static IServiceCollection AddServiceConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi();

        services.AddSerilog((serviceProvider, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(serviceProvider));

        services.ConfigureOptions(configuration);
        services.ConfigureDbContext();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IValidator<ActivateUserRequest>, ActivateUserRequestValidator>();
        services.AddScoped<IValidator<CheckUserRequest>, CheckUserRequestValidator>();

        services.AddJwtAuthentication(configuration);
        services.AddAuthorization();

        return services;
    }
}
