using MediatR;
using Pandora.App.Features.Start;
using Pandora.App.Services;
using Pandora.Core.Interfaces;
using Pandora.Infrastructure.Behaviors;

namespace Pandora.TgBot.Extension.DependencyInjection;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<StartMessage>());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SessionBehavior<,>));
        services.AddScoped<ISessionService, SessionService>();

        return services;
    }
}
