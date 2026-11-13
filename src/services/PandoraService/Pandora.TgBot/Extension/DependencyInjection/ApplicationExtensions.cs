using CoreSolution.Shared.Application.Behaviors;
using CoreSolution.Shared.TgBot.Abstractions;
using MediatR;
using Pandora.Domain.Constants;
using Pandora.Domain.Enums;
using Pandora.TgBot.Application.Features.Activation;
using Pandora.TgBot.ErrorHandling;

namespace Pandora.TgBot.Extension.DependencyInjection;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SubmitPhoneHandler>());
        services.AddSingleton(new SessionBehaviorOptions(nameof(WorkflowStep.ServiceSelection), PandoraConstants.ServiceName));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SessionBehavior<,>));
        services.AddScoped<IUserErrorMapper, PandoraUserErrorMapper>();

        return services;
    }
}
