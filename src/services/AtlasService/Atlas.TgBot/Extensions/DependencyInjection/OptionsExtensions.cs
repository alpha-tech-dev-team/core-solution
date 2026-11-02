using Atlas.Infrastructure.Options;
using SessionOptions = Atlas.Core.Options.SessionOptions;
using Atlas.Core.Options;
using StackExchange.Redis;

namespace Atlas.TgBot;

public static class OptionsExtensions
{
    public static IServiceCollection AddIdentityOptions(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configuration[ConfigurationKeys.TelegramBotToken]);

        var databaseSection = configuration.GetSection(DatabaseOptions.SectionName);
        services.Configure<DatabaseOptions>(databaseSection);
        var databaseOptions = databaseSection.Get<DatabaseOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {DatabaseOptions.SectionName}.");

        services.AddSingleton<IConnectionMultiplexer>(_ => 
            ConnectionMultiplexer.Connect(databaseOptions.RedisConnection));


        var identitySection = configuration.GetSection(IdentityOptions.SectionName);
        services.Configure<IdentityOptions>(identitySection);
        services.Configure<SessionOptions>(configuration.GetSection(SessionOptions.SectionName));

        return services;
    }
}
