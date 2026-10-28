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

        services.Configure<AccessOptions>(
            configuration.GetSection(AccessOptions.SectionName));

        services.Configure<IdentityOptions>(
            configuration.GetSection(IdentityOptions.SectionName));

        services.Configure<Atlas.Core.Options.SessionOptions>(
            configuration.GetSection(
                Atlas.Core.Options.SessionOptions.SectionName));

        services.Configure<DatabaseOptions>(
            configuration.GetSection(DatabaseOptions.SectionName));

        return services;
    }
}
