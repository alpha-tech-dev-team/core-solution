using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pgvector.EntityFrameworkCore;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class KnowledgeArticleVectorConfiguration : IEntityTypeConfiguration<KnowledgeArticleVector>
{
    public void Configure(EntityTypeBuilder<KnowledgeArticleVector> builder)
    {
        builder.ToTable("knowledge_article_vectors", "atlas");

        builder.HasKey(x => x.ArticleId).HasName("knowledge_article_vectors_pkey");
        builder.Property(x => x.ArticleId)
            .HasColumnName("article_id");

        builder.Property(x => x.Embedding)
            .HasColumnName("embedding")
            .HasColumnType("vector(1536)")
            .IsRequired();

        builder.Property(x => x.EmbeddingModel)
            .HasColumnName("embedding_model")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.EmbeddedText)
            .HasColumnName("embedded_text")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        builder.HasIndex(x => x.Embedding)
            .HasMethod("hnsw")
            .HasOperators("vector_cosine_ops")
            .HasDatabaseName("idx_article_vectors_embedding");

        builder.HasOne(x => x.Article)
            .WithOne(x => x.Vector)
            .HasForeignKey<KnowledgeArticleVector>(x => x.ArticleId)
            .HasConstraintName("knowledge_article_vectors_article_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
