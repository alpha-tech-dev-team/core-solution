namespace Atlas.Domain.Entities;

public sealed class KnowledgeArticle
{
    public Guid Id { get; set; }
    public Guid? CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Symptoms { get; set; }
    public string Solution { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? CreatedBy { get; set; }
    public Guid? SourceIncidentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public IncidentCategory? Category { get; set; }
    public EmployeeAccess? CreatedByEmployee { get; set; }
    public Incident? SourceIncident { get; set; }
    public KnowledgeArticleVector? Vector { get; set; }
    public ICollection<IncidentFeedback> Feedback { get; set; } = new List<IncidentFeedback>();
}
