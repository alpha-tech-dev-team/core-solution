using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pandora.Infrastructure.Secrets;

namespace Pandora.TgBot.Extension.DependencyInjection;

public static class OptionsExtensions
{
    public static IServiceCollection AddPandoraOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SecretsOptions>(configuration.GetSection(SecretsOptions.SectionName));

        return services;
    }
}
