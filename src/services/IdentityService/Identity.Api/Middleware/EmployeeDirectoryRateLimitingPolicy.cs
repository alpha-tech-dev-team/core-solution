using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Identity.Api.Middleware;

public sealed class EmployeeDirectoryRateLimitingPolicy : IRateLimiterPolicy<string>
{
    private const string UnknownTelegramUserId = "unknown";

    private readonly EmployeeDirectoryRateLimitingOptions _options;

    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected { get; }

    public EmployeeDirectoryRateLimitingPolicy(IOptions<EmployeeDirectoryRateLimitingOptions> options)
    {
        _options = options.Value;
    }

    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        var partitionKey = httpContext.Request.RouteValues.TryGetValue("telegramUserId", out var routeValue)
            && routeValue is not null
                ? routeValue.ToString() ?? UnknownTelegramUserId
                : UnknownTelegramUserId;

        return RateLimitPartition.GetTokenBucketLimiter(
            partitionKey,
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
