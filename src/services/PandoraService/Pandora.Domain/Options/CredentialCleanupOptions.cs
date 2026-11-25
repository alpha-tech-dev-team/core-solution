using Pandora.Domain.Constants;

namespace Pandora.Domain.Options;

public sealed class CredentialCleanupOptions
{
    public const string SectionName = "Pandora:Credentials:Cleanup";

    public bool Enabled { get; set; } = true;
    public int PollIntervalSeconds { get; set; } = 15;
    public int BatchSize { get; set; } = 50;
    public int CompletedRetentionHours { get; set; } = 24;
    public int DeleteAttemptsBeforeFallback { get; set; } = 3;
    public int MaxAttempts { get; set; } = 10;
    public int RetryBaseDelaySeconds { get; set; } = 30;
    public int MaxRetryDelaySeconds { get; set; } = 300;

    public void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(PollIntervalSeconds, CredentialAccessConstants.MinCleanupPollIntervalSeconds);
        ArgumentOutOfRangeException.ThrowIfLessThan(BatchSize, CredentialAccessConstants.MinCleanupBatchSize);
        ArgumentOutOfRangeException.ThrowIfLessThan(CompletedRetentionHours, CredentialAccessConstants.MinCompletedRetentionHours);
        ArgumentOutOfRangeException.ThrowIfLessThan(DeleteAttemptsBeforeFallback, CredentialAccessConstants.MinDeleteAttemptsBeforeFallback);
        ArgumentOutOfRangeException.ThrowIfLessThan(MaxAttempts, CredentialAccessConstants.MinCleanupMaxAttempts);
        ArgumentOutOfRangeException.ThrowIfLessThan(RetryBaseDelaySeconds, CredentialAccessConstants.MinRetryDelaySeconds);
        ArgumentOutOfRangeException.ThrowIfLessThan(MaxRetryDelaySeconds, CredentialAccessConstants.MinMaxRetryDelaySeconds);

        if (MaxRetryDelaySeconds < RetryBaseDelaySeconds)
        {
            throw new ArgumentException("Maximum retry delay must not be less than the retry base delay.");
        }
    }
}
