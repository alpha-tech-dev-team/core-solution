using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pandora.Domain.Options;

namespace Pandora.TgBot.Extension.DependencyInjection;

public static class OptionsExtensions
{
    public static IServiceCollection AddPandoraOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SecretsOptions>()
            .Bind(configuration.GetSection(SecretsOptions.SectionName));

        services.AddOptions<CredentialCleanupOptions>()
            .Bind(configuration.GetSection(CredentialCleanupOptions.SectionName))
            .PostConfigure(options => options.Validate())
            .ValidateOnStart();

        return services;
    }
}
