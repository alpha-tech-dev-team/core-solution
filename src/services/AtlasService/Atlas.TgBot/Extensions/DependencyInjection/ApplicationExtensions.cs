using Atlas.Application.Abstractions;
using Atlas.Application.Behaviors;
using Atlas.Application.Features.Activation;
using Atlas.Domain.Constants;
using Atlas.TgBot.Features.Activation;
using Atlas.TgBot.Localization;
using Atlas.TgBot.Notifications;
using Atlas.TgBot.Options;
using CoreSolution.Shared.Application.Behaviors;
using CoreSolution.Shared.Application.Messages;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using CoreSolution.Shared.TgBot.Abstractions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class ApplicationExtensions
{
    private const string NotificationBotClientServiceKey = "notification";
    private const string NotificationBotTokenKey = "Telegram:NotificationBotToken";

    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SubmitPhoneHandler>());
        services.AddValidatorsFromAssemblyContaining<SubmitPhoneHandler>();
        services.AddSingleton(new SessionBehaviorOptions(AtlasConstants.InitialWorkflowStep, AtlasConstants.ServiceName));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SessionBehavior<,>));
        services.AddScoped<IUserErrorMapper, AtlasUserErrorMapper>();
        var notificationBotToken = configuration[NotificationBotTokenKey];
        ArgumentException.ThrowIfNullOrWhiteSpace(notificationBotToken);
        services.AddKeyedSingleton<ITelegramBotClient>(
            NotificationBotClientServiceKey,
            (_, _) => new TelegramBotClient(notificationBotToken));
        services.AddSingleton<ITelegramMessageSender>(provider => new TelegramMessageSender(
            provider.GetRequiredKeyedService<ITelegramBotClient>(NotificationBotClientServiceKey),
            provider.GetRequiredService<ILogger<TelegramMessageSender>>()));

        services.Configure<PhoneConfirmationOptions>(configuration.GetSection(PhoneConfirmationOptions.SectionName));

        RegisterSubmitPhoneHandler(services, configuration, environment);

        return services;
    }

    private static void RegisterSubmitPhoneHandler(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var options = configuration.GetSection(PhoneConfirmationOptions.SectionName).Get<PhoneConfirmationOptions>()
            ?? new PhoneConfirmationOptions();

        if (options.Enabled || environment.IsProduction())
        {
            return;
        }

        services.Replace(ServiceDescriptor.Scoped<IRequestHandler<SubmitPhoneMessage, Result<WorkflowResponse>>, SkipSubmitPhoneHandler>());
    }
}
