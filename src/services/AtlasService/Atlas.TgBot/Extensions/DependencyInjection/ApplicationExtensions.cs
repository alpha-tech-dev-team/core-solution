using Atlas.Application.Behaviors;
using Atlas.Application.Features.Activation;
using Atlas.Domain.Constants;
using Atlas.TgBot.Localization;
using CoreSolution.Shared.Application.Behaviors;
using CoreSolution.Shared.TgBot.Abstractions;
using FluentValidation;
using MediatR;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SubmitPhoneHandler>());
        services.AddValidatorsFromAssemblyContaining<SubmitPhoneHandler>();
        services.AddSingleton(new SessionBehaviorOptions(AtlasConstants.InitialWorkflowStep, AtlasConstants.ServiceName));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SessionBehavior<,>));
        services.AddScoped<IUserErrorMapper, AtlasUserErrorMapper>();

        return services;
    }
}
