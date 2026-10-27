using System.Threading.RateLimiting;
using Identity.Core.Options;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Identity.Api.Middleware;

public class AuthRateLimitingPolicy : IRateLimiterPolicy<string>
{
    private const string UnknownIpAddress = "unknown";
    
    private readonly RateLimitingOptions _options;

    public AuthRateLimitingPolicy(IOptions<RateLimitingOptions> options)
    {
        _options = options.Value;
    }

    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected { get; }

    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        return RateLimitPartition.GetTokenBucketLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? UnknownIpAddress,
            _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = _options.TokenLimit,
                TokensPerPeriod = _options.TokensPerPeriod,
                ReplenishmentPeriod = TimeSpan.FromSeconds(_options.ReplenishmentPeriodSeconds),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    }
}
