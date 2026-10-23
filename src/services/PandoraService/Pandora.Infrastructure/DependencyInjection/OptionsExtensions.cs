using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pandora.Core.Options;
using Pandora.Infrastructure.Identity;
using Pandora.Infrastructure.Secrets;

namespace Pandora.Infrastructure.DependencyInjection;

public static class OptionsExtensions
{
    public static IServiceCollection AddPandoraOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SessionOptions>(configuration.GetSection(SessionOptions.SectionName));
        services.Configure<IdentityOptions>(configuration.GetSection(IdentityOptions.SectionName));
        services.Configure<SecretsOptions>(configuration.GetSection(SecretsOptions.SectionName));

        return services;
    }
}
