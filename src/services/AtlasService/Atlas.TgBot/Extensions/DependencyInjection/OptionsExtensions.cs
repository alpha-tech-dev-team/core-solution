using Atlas.Core.Interfaces;
using Atlas.Infrastructure.Identity;
using Atlas.Infrastructure.Options;
using SessionOptions = Atlas.Core.Options.SessionOptions;

namespace Atlas.TgBot;

public static class OptionsExtensions
{
    public static IServiceCollection AddIdentityOptions(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configuration[ConfigurationKeys.TelegramBotToken]);

        var identitySection = configuration.GetSection(IdentityOptions.SectionName);
        services.Configure<IdentityOptions>(identitySection);
        services.Configure<SessionOptions>(configuration.GetSection(SessionOptions.SectionName));

        var identityOptions = identitySection.Get<IdentityOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {IdentityOptions.SectionName}.");
        ArgumentException.ThrowIfNullOrWhiteSpace(identityOptions.BaseUrl);

        services.AddHttpClient<IIdentityClient, IdentityClient>(client =>
        {
            client.BaseAddress = new Uri(identityOptions.BaseUrl);
        });

        return services;
    }
}
