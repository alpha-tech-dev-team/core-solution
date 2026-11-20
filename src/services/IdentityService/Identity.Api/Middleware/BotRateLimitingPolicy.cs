using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Identity.Api.Middleware;

public sealed class BotRateLimitingPolicy : IRateLimiterPolicy<string>
{
    private const string UnknownIpAddress = "unknown";

    private readonly BotRateLimitingOptions _options;

    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected { get; }

    public BotRateLimitingPolicy(IOptions<BotRateLimitingOptions> options)
    {
        _options = options.Value;
    }

    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        return RateLimitPartition.GetTokenBucketLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? UnknownIpAddress,
            _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = _options.TokenLimit,
                TokensPerPeriod = _options.TokensPerPeriod,
                ReplenishmentPeriod = TimeSpan.FromSeconds(_options.ReplenishmentPeriodSeconds),
                QueueLimit = _options.QueueLimit,
                AutoReplenishment = _options.AutoReplenishment
            });
    }
}
