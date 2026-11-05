namespace Atlas.Domain.Entities;

public sealed class IncidentCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
    public ICollection<KnowledgeArticle> Articles { get; set; } = new List<KnowledgeArticle>();
}
