namespace Pandora.Domain.Options;

public sealed class SecretSyncOptions
{
    public const string SectionName = "Pandora:Secrets:Sync";

    public bool Enabled { get; set; } = true;
    public int IntervalSeconds { get; set; } = 60;
    public int BatchSize { get; set; } = 50;
    public int MaxAttempts { get; set; } = 5;
    public int MaxRetryDelaySeconds { get; set; } = 3600;
    public int MaxParallelism { get; set; } = 1;
}
