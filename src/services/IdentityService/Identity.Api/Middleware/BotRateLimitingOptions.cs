namespace Identity.Api.Middleware;

public sealed class BotRateLimitingOptions
{
    public const string SectionName = "RateLimiting:Bot";

    public int TokenLimit { get; set; } = 120;
    public int TokensPerPeriod { get; set; } = 120;
    public int ReplenishmentPeriodSeconds { get; set; } = 60;
    public int QueueLimit { get; set; }
    public bool AutoReplenishment { get; set; } = true;
}
