using System.Text;
using CoreSolution.Shared.Options;
using Pandora.Domain.Constants;
using Pandora.Domain.Options;

namespace Pandora.Api.Extensions.Configuration;

public static class OptionsConfigurationExtensions
{
    public static IServiceCollection AddOptionsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterOptions(configuration);
        services.ValidateOptions();

        return services;
    }

    private static IServiceCollection RegisterOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DatabaseOptions>().Bind(configuration.GetSection(DatabaseOptions.SectionName));
        services.AddOptions<JwtOptions>().Bind(configuration.GetSection(JwtOptions.SectionName));
        services.AddOptions<SecretSyncOptions>().Bind(configuration.GetSection(SecretSyncOptions.SectionName));
        services.AddOptions<VaultwardenOptions>().Bind(configuration.GetSection(VaultwardenOptions.SectionName));

        return services;
    }

    private static IServiceCollection ValidateOptions(this IServiceCollection services)
    {
        services.AddOptions<DatabaseOptions>()
            .PostConfigure(options => ArgumentException.ThrowIfNullOrWhiteSpace(options.ConnectionString))
            .ValidateOnStart();

        services.AddOptions<JwtOptions>()
            .PostConfigure(options =>
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(options.Secret);
                ArgumentException.ThrowIfNullOrWhiteSpace(options.Issuer);
                ArgumentException.ThrowIfNullOrWhiteSpace(options.Audience);
                ArgumentOutOfRangeException.ThrowIfLessThan(
                    Encoding.UTF8.GetByteCount(options.Secret),
                    JwtOptions.MinimumSecretLength);
            })
            .ValidateOnStart();

        services.AddOptions<SecretSyncOptions>()
            .PostConfigure(options =>
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(options.IntervalSeconds, SecretSyncLimits.MinIntervalSeconds);
                ArgumentOutOfRangeException.ThrowIfLessThan(options.BatchSize, SecretSyncLimits.MinBatchSize);
                ArgumentOutOfRangeException.ThrowIfGreaterThan(options.BatchSize, SecretSyncLimits.MaxBatchSize);
                ArgumentOutOfRangeException.ThrowIfLessThan(options.MaxAttempts, SecretSyncLimits.MinAttempts);
                ArgumentOutOfRangeException.ThrowIfGreaterThan(options.MaxAttempts, SecretSyncLimits.MaxAttemptsLimit);
                ArgumentOutOfRangeException.ThrowIfLessThan(options.MaxParallelism, SecretSyncLimits.MinParallelism);
                ArgumentOutOfRangeException.ThrowIfGreaterThan(options.MaxParallelism, SecretSyncLimits.MaxParallelismLimit);
                ArgumentOutOfRangeException.ThrowIfLessThan(options.MaxRetryDelaySeconds, SecretSyncLimits.MinRetryDelaySeconds);
            })
            .ValidateOnStart();

        services.AddOptions<VaultwardenOptions>()
            .PostConfigure(options => ArgumentException.ThrowIfNullOrWhiteSpace(options.BaseUrl))
            .ValidateOnStart();

        return services;
    }
}
