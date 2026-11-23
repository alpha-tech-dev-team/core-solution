using Atlas.Domain.Interfaces.Repositories;
using Atlas.Infrastructure.Repositories;

namespace Atlas.Api.Extensions.Configuration;

public static class RepositoryConfigurationExtensions
{
    public static IServiceCollection AddRepositoriesConfiguration(this IServiceCollection services)
    {
        services.AddScoped<IIncidentRepository, IncidentRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();

        return services;
    }
}
