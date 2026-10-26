using System.Runtime.CompilerServices;
using Identity.Core.Options;
using Identity.Infrastructure.Security;

namespace Identity.Infrastructure.Options.Extensions;

public static class OptionsConfigurationExtensions
{
    public static IServiceCollection ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));

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

    private static void ValidateInRange(int value, int min, int max, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, min, paramName);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, max, paramName);
    }
}
