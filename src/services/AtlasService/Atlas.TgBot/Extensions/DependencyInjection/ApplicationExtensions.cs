using MediatR;
using Atlas.App.Features.Start;
using Atlas.App.Services;
using Atlas.Core.Interfaces;
using Atlas.Infrastructure.Behaviors;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<StartRequest>());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SessionBehavior<,>));
        services.AddScoped<ISessionService, SessionService>();

        return services;
    }
}
