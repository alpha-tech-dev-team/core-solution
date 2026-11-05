using Pgvector;

namespace Atlas.Domain.Entities;

public sealed class KnowledgeArticleVector
{
    public Guid ArticleId { get; set; }
    public Vector Embedding { get; set; } = default!;
    public string EmbeddingModel { get; set; } = string.Empty;
    public string EmbeddedText { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }

    public KnowledgeArticle Article { get; set; } = default!;
}
