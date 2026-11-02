using Atlas.Core.Interfaces;
using Atlas.Core.Options;
using Atlas.Infrastructure.Options;
using SessionOptions = Atlas.Core.Options.SessionOptions;

namespace Atlas.TgBot;

public static class OptionsExtensions
{
    public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configuration[ConfigurationKeys.TelegramBotToken]);
        
        var databaseSection = configuration.GetSection(DatabaseOptions.SectionName);
        services.Configure<DatabaseOptions>(databaseSection);
        
        var databaseOptions = databaseSection.Get<DatabaseOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {DatabaseOptions.SectionName}.");

        var identitySection = configuration.GetSection(IdentityOptions.SectionName);
        services.Configure<IdentityOptions>(identitySection);
        services.Configure<SessionOptions>(configuration.GetSection(SessionOptions.SectionName));

        var identityOptions = identitySection.Get<IdentityOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {IdentityOptions.SectionName}.");
        ArgumentException.ThrowIfNullOrWhiteSpace(identityOptions.BaseUrl);
                
        return services;
    }
}
