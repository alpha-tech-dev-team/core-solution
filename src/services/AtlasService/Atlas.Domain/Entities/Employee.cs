using Atlas.Domain.Enums;

namespace Atlas.Domain.Entities;

public sealed class Employee
{
    public long Id { get; set; }
    public EmployeeRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Incident> EmployeeIncidents { get; set; } = new List<Incident>();
    public ICollection<Incident> AssignedIncidents { get; set; } = new List<Incident>();
    public ICollection<IncidentStatusHistory> StatusChanges { get; set; } = new List<IncidentStatusHistory>();
    public ICollection<KnowledgeArticle> CreatedArticles { get; set; } = new List<KnowledgeArticle>();
}
