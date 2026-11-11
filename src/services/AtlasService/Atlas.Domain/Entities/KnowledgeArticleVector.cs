namespace Atlas.Domain.Entities;

public sealed class KnowledgeArticleVector
{
    public Guid ArticleId { get; set; }
    public float[] Embedding { get; set; } = [];
    public string EmbeddingModel { get; set; } = string.Empty;
    public string EmbeddedText { get; set; } = string.Empty;

    public KnowledgeArticle Article { get; set; } = default!;
}
