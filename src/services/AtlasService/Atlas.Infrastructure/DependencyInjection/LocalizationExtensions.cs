using Atlas.Core.Interfaces;
using Atlas.Infrastructure.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Infrastructure.DependencyInjection;

public static class LocalizationExtensions
{
    public static IServiceCollection AddLocalizationResources(this IServiceCollection services)
    {
        services.AddSingleton<LocalizationProvider>();
        services.AddScoped<ILocalizer, Localizer>();

        return services;
    }
}
