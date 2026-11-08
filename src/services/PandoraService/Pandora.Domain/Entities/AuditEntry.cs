namespace Pandora.Domain.Entities;

public sealed class AuditEntry
{
    public AuditEntry(Guid id, DateTime timestamp, Guid userId, string action, string? targetItemId, string requestSource)
    {
        Id = id;
        Timestamp = timestamp;
        UserId = userId;
        Action = action;
        TargetItemId = targetItemId;
        RequestSource = requestSource;
    }

    public Guid Id { get; }
    public DateTime Timestamp { get; }
    public Guid UserId { get; }
    public string Action { get; }
    public string? TargetItemId { get; }
    public string RequestSource { get; }
}
