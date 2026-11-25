using Identity.Api.Constants;
using Identity.Domain.Options;
using Identity.Api.Middleware;

namespace Identity.Api.Extensions.Configuration;

public static class RateLimitingConfigurationExtensions
{
    public static IServiceCollection AddRateLimitingConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(RateLimitingOptions.SectionName);
        var options = section.Get<RateLimitingOptions>() ?? new RateLimitingOptions();

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(options.TokenLimit, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(options.TokensPerPeriod, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(options.ReplenishmentPeriodSeconds, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(options.QueueLimit, 0);

        var botSection = configuration.GetSection(BotRateLimitingOptions.SectionName);
        var botOptions = botSection.Get<BotRateLimitingOptions>() ?? new BotRateLimitingOptions();

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(botOptions.TokenLimit, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(botOptions.TokensPerPeriod, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(botOptions.ReplenishmentPeriodSeconds, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(botOptions.QueueLimit, 0);

        var employeeDirectorySection = configuration.GetSection(EmployeeDirectoryRateLimitingOptions.SectionName);
        var employeeDirectoryOptions = employeeDirectorySection.Get<EmployeeDirectoryRateLimitingOptions>()
            ?? new EmployeeDirectoryRateLimitingOptions();

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(employeeDirectoryOptions.TokenLimit, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(employeeDirectoryOptions.TokensPerPeriod, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(employeeDirectoryOptions.ReplenishmentPeriodSeconds, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(employeeDirectoryOptions.QueueLimit, 0);

        services.Configure<RateLimitingOptions>(section);
        services.Configure<BotRateLimitingOptions>(botSection);
        services.Configure<EmployeeDirectoryRateLimitingOptions>(employeeDirectorySection);

        services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            limiter.AddPolicy<string, AuthRateLimitingPolicy>(RateLimitingPolicies.Auth);
            limiter.AddPolicy<string, BotRateLimitingPolicy>(RateLimitingPolicies.BotClient);
            limiter.AddPolicy<string, EmployeeDirectoryRateLimitingPolicy>(RateLimitingPolicies.EmployeeDirectory);
        });

        return services;
    }
}
