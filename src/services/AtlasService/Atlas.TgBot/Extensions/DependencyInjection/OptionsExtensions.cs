using Atlas.Core.Options;
using Atlas.Infrastructure.Options;

namespace Atlas.TgBot;

public static class OptionsExtensions
{
    public static IServiceCollection AddAtlasOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<TelegramBotOptions>(
            configuration.GetSection(TelegramBotOptions.SectionName));

        services.Configure<IdentityOptions>(
            configuration.GetSection(IdentityOptions.SectionName));

        services.Configure<Core.Options.SessionOptions>(
            configuration.GetSection(
                Core.Options.SessionOptions.SectionName));

        services.Configure<DatabaseOptions>(
            configuration.GetSection(DatabaseOptions.SectionName));

        return services;
    }
}
