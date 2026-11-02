using Atlas.Core.Interfaces;
using Atlas.Infrastructure.Localization;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class LocalizationExtensions
{
    public static IServiceCollection AddLocalizationExtensions(this IServiceCollection services)
    {
        services.AddSingleton<LocalizationProvider>();
        services.AddScoped<ILocalizer, Localizer>();

        return services;
    }
}
