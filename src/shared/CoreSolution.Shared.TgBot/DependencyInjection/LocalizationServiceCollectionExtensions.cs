using System.Reflection;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Infrastructure.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace CoreSolution.Shared.TgBot.DependencyInjection;

public static class LocalizationServiceCollectionExtensions
{
    public static IServiceCollection AddLocalization(this IServiceCollection services, Assembly resourceAssembly)
    {
        services.AddSingleton(_ => new LocalizationProvider(resourceAssembly));
        services.AddScoped<ILocalizer, Localizer>();

        return services;
    }
}
