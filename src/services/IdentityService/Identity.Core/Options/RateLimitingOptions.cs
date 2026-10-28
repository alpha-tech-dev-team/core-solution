namespace Identity.Core.Options;

public class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";
    public int TokenLimit { get; set; } = 10;
    public int TokensPerPeriod { get; set; } = 10;
    public int ReplenishmentPeriodSeconds { get; set; } = 60;
    public int QueueLimit { get; set; } = 0;
    public bool AutoReplenishment { get; set; } = true;
}
