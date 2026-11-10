using CoreSolution.Shared.Application.Behaviors;
using CoreSolution.Shared.TgBot.Abstractions;
using MediatR;
using Pandora.Application.Behaviors;
using Pandora.Application.Common.Services;
using Pandora.Application.Features.Activation;
using Pandora.Domain.Constants;
using Pandora.Domain.Enums;
using Pandora.TgBot.Localization;

namespace Pandora.TgBot.Extension.DependencyInjection;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SubmitPhoneHandler>());
        services.AddSingleton(new SessionBehaviorOptions(nameof(WorkflowStep.ServiceSelection), PandoraConstants.ServiceName));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SessionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UserActivityBehavior<,>));
        services.AddScoped<UserActivityAuditLogger>();
        services.AddScoped<IUserErrorMapper, PandoraUserErrorMapper>();

        return services;
    }
}
