using Microsoft.Extensions.DependencyInjection;
using Pandora.Core.Interfaces;
using Pandora.Infrastructure.Localization;

namespace Pandora.Infrastructure.DependencyInjection;

public static class LocalizationExtensions
{
    public static IServiceCollection AddLocalizationResources(this IServiceCollection services)
    {
        services.AddSingleton<LocalizationProvider>();
        services.AddScoped<ILocalizer, Localizer>();

        return services;
    }
}
