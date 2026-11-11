using Atlas.Domain.Enums;

namespace Atlas.Domain.Entities;

public sealed class IncidentStatusHistory
{
    public Guid Id { get; set; }
    public Guid IncidentId { get; set; }
    public IncidentStatus Status { get; set; }
    public long? LastChangedBy { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }

    public Incident Incident { get; set; } = default!;
    public Employee? LastChangedByEmployee { get; set; }
}
