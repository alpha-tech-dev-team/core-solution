namespace Pandora.Domain.Entities;

public sealed class AuditOutboxEntry
{
    public Guid Id { get; }
    public string Payload { get; }
    public DateTime CreatedAt { get; }

    public AuditOutboxEntry(Guid id, string payload, DateTime createdAt)
    {
        Id = id;
        Payload = payload;
        CreatedAt = createdAt;
    }
}
