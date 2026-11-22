using System.Text;
using Atlas.Domain.Options;
using CoreSolution.Shared.Options;

namespace Atlas.Api.Extensions.Configuration;

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

        return services;
    }
}
