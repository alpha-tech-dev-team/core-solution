using Atlas.Domain.Enums;

namespace Atlas.Domain.Models;

public sealed class IncidentDraft
{
    public WorkflowStep Step { get; set; }
    public Guid? CategoryId { get; set; }
    public DateTime UpdatedAt { get; set; }
}
