using Atlas.Core.Options;
using Atlas.Infrastructure.Options;
using Atlas.Infrastructure.Identity;
using Atlas.Core.Interfaces;
using Atlas.Infrastructure.Repositories;
using StackExchange.Redis;
using Atlas.Infrastructure.Utils;

namespace Atlas.TgBot;

public static class OptionsExtensions
{
    public static IServiceCollection AddAtlasOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        //TelegramBotOptions

        var telegramSection = configuration.GetSection(TelegramBotOptions.SectionName);        
        services.Configure<TelegramBotOptions>(telegramSection);

        var telegramOptions = telegramSection.Get<TelegramBotOptions>()
            ?? throw new InvalidOperationException($"Section {TelegramBotOptions.SectionName} is missing");

        if (string.IsNullOrWhiteSpace(telegramOptions.BotToken))
        {
            throw new InvalidOperationException($"{TelegramBotOptions.SectionName}:{nameof(TelegramBotOptions.BotToken)} is missing");
        }

        //IdentityOptions

        var identitySection = configuration.GetSection(IdentityOptions.SectionName);
        services.Configure<IdentityOptions>(identitySection);

        services.Configure<Core.Options.SessionOptions>(
            configuration.GetSection(Core.Options.SessionOptions.SectionName));

        var identityOptions = identitySection.Get<IdentityOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {IdentityOptions.SectionName}.");

        ArgumentException.ThrowIfNullOrWhiteSpace(identityOptions.BaseUrl);

        services.AddHttpClient<IIdentityClient, IdentityClient>(client =>
        {
            client.BaseAddress = new Uri(identityOptions.BaseUrl);
            client.Timeout = Timeout.InfiniteTimeSpan;
        });

        //DatabaseOptions

        var databaseSection = configuration.GetSection(DatabaseOptions.SectionName);
        services.Configure<DatabaseOptions>(databaseSection);
        
        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {DatabaseOptions.SectionName}.");

        services.AddSingleton<IConnectionMultiplexer>(_ => 
            ConnectionMultiplexer.Connect(databaseOptions.RedisConnection));
            
        services.AddScoped<ISessionRepository, RedisSessionRepository>();
        services.AddScoped<IRequestContext, RequestContext>();

        return services;    }
}
