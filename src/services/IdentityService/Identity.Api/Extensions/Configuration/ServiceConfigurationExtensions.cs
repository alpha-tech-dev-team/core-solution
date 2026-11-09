using FluentValidation;
using FluentValidation.AspNetCore;
using Identity.Api.Middleware;
using Identity.Application.Services;
using Identity.Domain.Interfaces;
using Identity.Domain.Interfaces.Repositories;
using Identity.Domain.Interfaces.Security;
using Identity.Domain.Interfaces.Services;
using Identity.Infrastructure.Database;
using Identity.Infrastructure.Database.Extensions;
using Identity.Infrastructure.Mapping;
using Identity.Infrastructure.Options.Extensions;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Services;
using Mapster;
using ApplicationAssemblyMarker = Identity.Application.AssemblyMarker;
using Serilog;

namespace Identity.Api.Extensions.Configuration;

public static class ServiceConfigurationExtensions
{
    public static IServiceCollection AddServiceConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        TypeAdapterConfig.GlobalSettings.Apply(new EmployeeMappingConfig());

        services.AddOpenApi();

        services.AddSerilog((serviceProvider, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(serviceProvider));

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddControllers();

        services.ConfigureOptions(configuration);
        services.ConfigureDbContext();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IAdminAuthService, AdminAuthService>();
        services.AddScoped<IAdminUserService, AdminUserService>();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>();
        services.AddFluentValidationAutoValidation();

        services.AddJwtAuthentication();
        services.AddRateLimitingConfiguration(configuration);

        return services;
    }
}
