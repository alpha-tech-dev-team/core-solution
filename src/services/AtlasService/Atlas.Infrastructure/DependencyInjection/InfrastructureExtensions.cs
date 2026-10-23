using Atlas.Core.Interfaces;
using Atlas.Infrastructure.Access;
using Atlas.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.DependencyInjection;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddAccessControl(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AccessOptions>(configuration.GetSection(AccessOptions.SectionName));
        services.AddSingleton<IAccessService, AccessService>();
        return services;
    }
}