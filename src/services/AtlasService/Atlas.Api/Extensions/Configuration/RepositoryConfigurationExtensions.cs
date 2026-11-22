using Atlas.Domain.Interfaces.Repositories;
using Atlas.Infrastructure.Repositories;

namespace Atlas.Api.Extensions.Configuration;

public static class RepositoryConfigurationExtensions
{
    public static IServiceCollection AddRepositoriesConfiguration(this IServiceCollection services)
    {
        services.AddScoped<IIncidentRepository, IncidentRepository>();
        services.AddScoped<IIncidentCategoryRepository, IncidentCategoryRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IHardwareRepository, HardwareRepository>();
        services.AddScoped<ISoftwareRepository, SoftwareRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();

        return services;
    }
}
