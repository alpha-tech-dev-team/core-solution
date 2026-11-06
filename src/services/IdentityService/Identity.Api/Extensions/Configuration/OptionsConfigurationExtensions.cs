using System.Runtime.CompilerServices;
using System.Text;
using CoreSolution.Shared.Options;
using Identity.Domain.Options;
using Identity.Infrastructure.Constants;

namespace Identity.Infrastructure.Options.Extensions;

public static class OptionsConfigurationExtensions
{
    public static IServiceCollection ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(AreValidJwtOptions, JwtOptions.InvalidOptionsMessage)
            .ValidateOnStart();

        var section = configuration.GetSection(PasswordHasherOptions.SectionName);
        var options = section.Get<PasswordHasherOptions>() ?? new PasswordHasherOptions();

        ValidateInRange(options.SaltSize, PasswordHasherConstants.MinSaltSize, PasswordHasherConstants.MaxSaltSize);
        ValidateInRange(options.HashSize, PasswordHasherConstants.MinHashSize, PasswordHasherConstants.MaxHashSize);
        ValidateInRange(options.MemorySize, PasswordHasherConstants.MinMemorySize, PasswordHasherConstants.MaxMemorySize);
        ValidateInRange(options.Iterations, PasswordHasherConstants.MinIterations, PasswordHasherConstants.MaxIterations);
        ValidateInRange(options.Parallelism, PasswordHasherConstants.MinParallelism, PasswordHasherConstants.MaxParallelism);

        services.Configure<PasswordHasherOptions>(section);

        return services;
    }

    private static bool AreValidJwtOptions(JwtOptions options)
    {
        return !string.IsNullOrWhiteSpace(options.Secret)
            && Encoding.UTF8.GetByteCount(options.Secret) >= JwtOptions.MinimumSecretLength
            && !string.IsNullOrWhiteSpace(options.Issuer)
            && !string.IsNullOrWhiteSpace(options.Audience)
            && options.AccessTokenLifetimeMinutes > 0
            && options.RefreshTokenLifetimeDays > 0;
    }

    private static void ValidateInRange(int value, int min, int max, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, min, paramName);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, max, paramName);
    }
}
