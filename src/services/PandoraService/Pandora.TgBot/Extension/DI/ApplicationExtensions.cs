using Microsoft.Extensions.DependencyInjection;
using Pandora.App.Features.Start;

namespace Pandora.TgBot.Extension.DI;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<StartMessage>());

        return services;
    }
}
