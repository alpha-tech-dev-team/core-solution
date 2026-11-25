using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pandora.Domain.Interfaces;
using Pandora.TgBot.Application.Features.Credentials;
using Pandora.TgBot.Credentials;

namespace Pandora.TgBot.Extension.DependencyInjection;

public static class CredentialExtensions
{
    public static IServiceCollection AddCredentialCleanup(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<CredentialAccessNoticeBuilder>();
        services.AddScoped<ICredentialMessageGateway, TelegramCredentialMessageGateway>();
        services.AddScoped<ICredentialMessageNotifier, TelegramCredentialMessageNotifier>();
        services.AddScoped<CredentialMessageScheduler>();
        services.AddScoped<CredentialMessageCleanupService>();
        services.AddHostedService<CredentialMessageCleanupWorker>();

        return services;
    }
}
