namespace Pandora.Domain.Constants;

public static class SecretSyncLimits
{
    public const int MaxBackoffExponent = 16;
    public const int MinBatchSize = 1;
    public const int MaxBatchSize = 500;
    public const int MinAttempts = 1;
    public const int MaxAttemptsLimit = 100;
    public const int MinParallelism = 1;
    public const int MaxParallelismLimit = 16;
    public const int MinIntervalSeconds = 1;
    public const int MinRetryDelaySeconds = 1;
}
