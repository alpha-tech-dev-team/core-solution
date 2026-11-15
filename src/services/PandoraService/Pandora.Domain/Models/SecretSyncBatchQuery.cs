namespace Pandora.Domain.Models;

public sealed record SecretSyncBatchQuery(int BatchSize, int MaxAttempts, DateTime UtcNow);
