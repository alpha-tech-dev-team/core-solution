namespace Pandora.Domain.Entities;

public sealed class AuditEntry
{
    public Guid Id { get; }
    public DateTime Timestamp { get; }
    public Guid? UserId { get; }
    public long? TelegramUserId { get; }
    public string Action { get; }
    public string? TargetItemId { get; }
    public string RequestSource { get; }
    public long? TargetTelegramUserId { get; }

    private AuditEntry(Guid id, DateTime timestamp, string action, string? targetItemId, string requestSource, long? targetTelegramUserId)
    {
        Id = id;
        Timestamp = timestamp;
        Action = action;
        TargetItemId = targetItemId;
        RequestSource = requestSource;
        TargetTelegramUserId = targetTelegramUserId;
    }

    public AuditEntry(Guid id, DateTime timestamp, Guid userId, string action, string? targetItemId, string requestSource, long? targetTelegramUserId = null)
        : this(id, timestamp, action, targetItemId, requestSource, targetTelegramUserId)
    {
        UserId = userId;
    }

    public AuditEntry(Guid id, DateTime timestamp, long telegramUserId, string action, string? targetItemId, string requestSource, long? targetTelegramUserId = null)
        : this(id, timestamp, action, targetItemId, requestSource, targetTelegramUserId)
    {
        TelegramUserId = telegramUserId;
    }
}
