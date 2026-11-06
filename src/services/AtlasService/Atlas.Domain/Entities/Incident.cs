using Atlas.Domain.Enums;
using Atlas.Domain.Interfaces;

namespace Atlas.Domain.Entities;

public sealed class Incident : IHasUpdatedAt
{
    public Guid Id { get; set; }
    public long TicketNumber { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? CabinetId { get; set; }
    public Guid? HardwareId { get; set; }
    public Guid? SoftwareId { get; set; }
    public string UserDescription { get; set; } = string.Empty;
    public IncidentStatus Status { get; set; } = IncidentStatus.Created;
    public string Priority { get; set; } = string.Empty;
    public Guid? AssignedEngineerId { get; set; }
    public Guid? ArticleId { get; set; }
    public string? Resolution { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Employee Employee { get; set; } = default!;
    public Employee? AssignedEngineer { get; set; }
    public IncidentCategory Category { get; set; } = default!;
    public Cabinet? Cabinet { get; set; }
    public Hardware? Hardware { get; set; }
    public Software? Software { get; set; }
    public KnowledgeArticle? Article { get; set; }
    public ICollection<IncidentStatusHistory> StatusHistory { get; set; } = new List<IncidentStatusHistory>();
    public ICollection<IncidentFeedback> Feedback { get; set; } = new List<IncidentFeedback>();
}
