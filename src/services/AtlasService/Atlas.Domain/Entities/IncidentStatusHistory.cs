namespace Atlas.Domain.Entities;

public sealed class IncidentStatusHistory
{
    public long Id { get; set; }
    public Guid IncidentId { get; set; }
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public Guid? ChangedBy { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }

    public Incident Incident { get; set; } = default!;
    public Employee? ChangedByEmployee { get; set; }
}
