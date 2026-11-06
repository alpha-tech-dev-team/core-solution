using Pgvector;

using Atlas.Domain.Interfaces;

namespace Atlas.Domain.Entities;

public sealed class KnowledgeArticleVector : IHasUpdatedAt
{
    public Guid ArticleId { get; set; }
    public Vector Embedding { get; set; } = default!;
    public string EmbeddingModel { get; set; } = string.Empty;
    public string EmbeddedText { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }

    public KnowledgeArticle Article { get; set; } = default!;
}
