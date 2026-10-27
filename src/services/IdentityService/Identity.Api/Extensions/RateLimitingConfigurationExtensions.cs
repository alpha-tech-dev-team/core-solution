using Identity.Api.Constants;
using Identity.Core.Options;
using Identity.Api.Middleware;
using Microsoft.AspNetCore.RateLimiting;

namespace Identity.Api.Extensions;

public static class RateLimitingConfigurationExtensions
{
    public static IServiceCollection AddRateLimitingConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(RateLimitingOptions.SectionName);
        var options = section.Get<RateLimitingOptions>() ?? new RateLimitingOptions();

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(options.TokenLimit, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(options.TokensPerPeriod, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(options.ReplenishmentPeriodSeconds, 0);

        services.Configure<RateLimitingOptions>(section);

        services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            limiter.AddPolicy<string, AuthRateLimitingPolicy>(RateLimitingPolicies.Auth);
        });

        return services;
    }
}
