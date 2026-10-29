using Atlas.Core.Interfaces;
using Atlas.Core.Options;
using Atlas.Infrastructure.Identity;
using Atlas.Infrastructure.Options;
using Atlas.Infrastructure.Repositories;
using Atlas.Infrastructure.Utils;
using StackExchange.Redis;
using SessionOptions = Atlas.Core.Options.SessionOptions;
namespace Atlas.TgBot;

public static class OptionsExtensions
{
    public static IServiceCollection AddAtlasOptions(this IServiceCollection services, IConfiguration configuration)
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

        var databaseSection = configuration.GetSection(DatabaseOptions.SectionName);
        services.Configure<DatabaseOptions>(databaseSection);
        var databaseOptions = databaseSection.Get<DatabaseOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {DatabaseOptions.SectionName}.");

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(databaseOptions.RedisConnection));
        services.AddScoped<ISessionRepository, RedisSessionRepository>();
        services.AddScoped<IRequestContext, RequestContext>();
        return services;
    }
}
