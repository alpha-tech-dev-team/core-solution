namespace Atlas.Domain.Entities;

public sealed class IncidentFeedback
{
    public Guid Id { get; set; }
    public Guid IncidentId { get; set; }
    public Guid ArticleId { get; set; }
    public bool WasHelpful { get; set; }
    public DateTime CreatedAt { get; set; }

    public Incident Incident { get; set; } = default!;
    public KnowledgeArticle Article { get; set; } = default!;
}
