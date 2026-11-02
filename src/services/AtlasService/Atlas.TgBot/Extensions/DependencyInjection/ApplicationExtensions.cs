using Atlas.Application.Features.Activation;
using Atlas.Core.Constants;
using Atlas.Core.Enums;
using CoreSolution.Shared.Application.Behaviors;
using MediatR;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SubmitPhoneHandler>());
        services.AddSingleton(new SessionBehaviorOptions(nameof(WorkflowStep.ProblemCategorySelection), AtlasConstants.ServiceName));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SessionBehavior<,>));

        return services;
    }
}
