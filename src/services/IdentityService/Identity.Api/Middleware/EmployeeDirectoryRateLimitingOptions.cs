namespace Identity.Api.Middleware;

public sealed class EmployeeDirectoryRateLimitingOptions
{
    public const string SectionName = "RateLimiting:EmployeeDirectory";

    public int TokenLimit { get; set; } = 60;
    public int TokensPerPeriod { get; set; } = 60;
    public int ReplenishmentPeriodSeconds { get; set; } = 60;
    public int QueueLimit { get; set; }
    public bool AutoReplenishment { get; set; } = true;
}
