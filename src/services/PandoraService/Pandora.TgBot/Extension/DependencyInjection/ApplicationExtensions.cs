using MediatR;
using Pandora.App.Features.Activation;
using Pandora.App.Services;
using Pandora.Core.Interfaces;
using Pandora.Infrastructure.Behaviors;

namespace Pandora.TgBot.Extension.DependencyInjection;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SubmitPhoneMessage>());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SessionBehavior<,>));
        services.AddScoped<ISessionService, SessionService>();

        return services;
    }
}
