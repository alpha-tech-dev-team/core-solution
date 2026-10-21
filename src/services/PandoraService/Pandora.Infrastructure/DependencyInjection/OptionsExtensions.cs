using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pandora.Infrastructure.Access;
using Pandora.Infrastructure.Secrets;

namespace Pandora.Infrastructure.DependencyInjection;

public static class OptionsExtensions
{
    public static IServiceCollection AddPandoraOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AccessOptions>(configuration.GetSection(AccessOptions.SectionName));
        services.Configure<SecretsOptions>(configuration.GetSection(SecretsOptions.SectionName));

        return services;
    }
}
