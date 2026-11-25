using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pandora.Domain.Constants;
using Pandora.Domain.Options;

namespace Pandora.TgBot.Extension.DependencyInjection;

public static class OptionsExtensions
{
    public static IServiceCollection AddPandoraOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterOptions(configuration);
        services.ValidateOptions();

        return services;
    }

    private static IServiceCollection RegisterOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SecretsOptions>()
            .Bind(configuration.GetSection(SecretsOptions.SectionName));

        services.AddOptions<CredentialCleanupOptions>()
            .Bind(configuration.GetSection(CredentialCleanupOptions.SectionName));

        return services;
    }

    private static IServiceCollection ValidateOptions(this IServiceCollection services)
    {
        services.AddOptions<CredentialCleanupOptions>()
            .PostConfigure(options =>
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(
                    options.PollIntervalSeconds,
                    CredentialAccessConstants.MinCleanupPollIntervalSeconds);
                ArgumentOutOfRangeException.ThrowIfLessThan(
                    options.BatchSize,
                    CredentialAccessConstants.MinCleanupBatchSize);
                ArgumentOutOfRangeException.ThrowIfLessThan(
                    options.CompletedRetentionHours,
                    CredentialAccessConstants.MinCompletedRetentionHours);
                ArgumentOutOfRangeException.ThrowIfLessThan(
                    options.DeleteAttemptsBeforeFallback,
                    CredentialAccessConstants.MinDeleteAttemptsBeforeFallback);
                ArgumentOutOfRangeException.ThrowIfLessThan(
                    options.MaxAttempts,
                    CredentialAccessConstants.MinCleanupMaxAttempts);
                ArgumentOutOfRangeException.ThrowIfLessThan(
                    options.RetryBaseDelaySeconds,
                    CredentialAccessConstants.MinRetryDelaySeconds);
                ArgumentOutOfRangeException.ThrowIfLessThan(
                    options.MaxRetryDelaySeconds,
                    CredentialAccessConstants.MinMaxRetryDelaySeconds);

                if (options.MaxRetryDelaySeconds < options.RetryBaseDelaySeconds)
                {
                    throw new ArgumentException("Maximum retry delay must not be less than the retry base delay.");
                }
            })
            .ValidateOnStart();

        return services;
    }
}
