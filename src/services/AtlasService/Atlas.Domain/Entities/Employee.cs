using Atlas.Domain.Interfaces;

namespace Atlas.Domain.Entities;

public sealed class Employee : IHasUpdatedAt
{
    public Guid EmployeeId { get; set; }
    public long? TelegramId { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Incident> EmployeeIncidents { get; set; } = new List<Incident>();
    public ICollection<Incident> AssignedIncidents { get; set; } = new List<Incident>();
    public ICollection<IncidentStatusHistory> StatusChanges { get; set; } = new List<IncidentStatusHistory>();
    public ICollection<KnowledgeArticle> CreatedArticles { get; set; } = new List<KnowledgeArticle>();
}
