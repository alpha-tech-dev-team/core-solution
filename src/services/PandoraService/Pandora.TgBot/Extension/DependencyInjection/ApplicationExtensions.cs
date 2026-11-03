using CoreSolution.Shared.Application.Behaviors;
using MediatR;
using Pandora.Application.Features.Activation;
using Pandora.Domain.Constants;
using Pandora.Domain.Enums;

namespace Pandora.TgBot.Extension.DependencyInjection;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SubmitPhoneHandler>());
        services.AddSingleton(new SessionBehaviorOptions(nameof(WorkflowStep.ServiceSelection), PandoraConstants.ServiceName));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SessionBehavior<,>));

        return services;
    }
}
