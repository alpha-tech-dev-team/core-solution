namespace Pandora.Domain.Options;

public sealed class CredentialCleanupOptions
{
    public const string SectionName = "Pandora:Credentials:Cleanup";

    public bool Enabled { get; set; } = true;
    public int PollIntervalSeconds { get; set; } = 15;
    public int BatchSize { get; set; } = 50;
    public int CompletedRetentionHours { get; set; } = 24;
    public int DeleteAttemptsBeforeFallback { get; set; } = 3;
    public int RetryBaseDelaySeconds { get; set; } = 30;
    public int MaxRetryDelaySeconds { get; set; } = 300;
}
