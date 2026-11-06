using Atlas.Application.Features.Activation;
using Atlas.Domain.Constants;
using Atlas.Domain.Enums;
using Atlas.TgBot.Localization;
using CoreSolution.Shared.Application.Behaviors;
using CoreSolution.Shared.TgBot.Abstractions;
using MediatR;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SubmitPhoneHandler>());
        services.AddSingleton(new SessionBehaviorOptions(nameof(WorkflowStep.Idle), AtlasConstants.ServiceName));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SessionBehavior<,>));
        services.AddScoped<IUserErrorMapper, AtlasUserErrorMapper>();

        return services;
    }
}
