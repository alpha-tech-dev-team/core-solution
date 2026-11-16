using Atlas.Domain.Interfaces;

namespace Atlas.Domain.Entities;

public sealed class IncidentCategory : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
    public ICollection<KnowledgeArticle> Articles { get; set; } = new List<KnowledgeArticle>();
}
