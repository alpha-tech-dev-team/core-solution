namespace Pandora.Domain.Constants;

public static class CredentialAccessConstants
{
    public const int DefaultTtlMinutes = 3;
    public const int SettingsRowId = 1;
    public const int QueueClaimDurationSeconds = 120;
    public const int MaxBackoffExponent = 10;
    public const int BackoffBase = 2;
    public const int MinCleanupPollIntervalSeconds = 1;
    public const int MinCleanupBatchSize = 1;
    public const int MinCompletedRetentionHours = 1;
    public const int MinDeleteAttemptsBeforeFallback = 1;
    public const int MinCleanupMaxAttempts = 1;
    public const int MinRetryDelaySeconds = 1;
    public const int MinMaxRetryDelaySeconds = 1;

    public static readonly IReadOnlyList<int> AllowedTtlMinutes = [2, 3, 5];
}
