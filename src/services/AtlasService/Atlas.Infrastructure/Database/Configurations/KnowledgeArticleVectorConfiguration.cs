using Atlas.Infrastructure.Database.Constants;
using Atlas.Domain.Entities;
using Atlas.Infrastructure.Database.ValueComparers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pgvector;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class KnowledgeArticleVectorConfiguration : IEntityTypeConfiguration<KnowledgeArticleVector>
{
    public void Configure(EntityTypeBuilder<KnowledgeArticleVector> builder)
    {
        builder.ToTable(DatabaseConstants.KnowledgeArticleVector.Table);

        builder.HasKey(x => x.ArticleId);

        builder.Property(x => x.ArticleId)
            .HasColumnName(DatabaseConstants.KnowledgeArticleVector.ArticleId);

        var embedding = builder.Property(x => x.Embedding)
            .HasColumnName(DatabaseConstants.KnowledgeArticleVector.Embedding)
            .HasConversion(value => new Vector(value), vector => vector.ToArray())
            .HasColumnType(DatabaseConstants.KnowledgeArticleVector.EmbeddingColumnType)
            .IsRequired();

        embedding.Metadata.SetValueComparer(FloatArrayValueComparer.Instance);

        builder.Property(x => x.EmbeddingModel)
            .HasColumnName(DatabaseConstants.KnowledgeArticleVector.EmbeddingModel)
            .HasMaxLength(DatabaseConstants.KnowledgeArticleVector.EmbeddingModelMaxLength)
            .IsRequired();

        builder.Property(x => x.EmbeddedText)
            .HasColumnName(DatabaseConstants.KnowledgeArticleVector.EmbeddedText)
            .IsRequired();

        builder.HasIndex(x => x.Embedding)
            .HasMethod(DatabaseConstants.KnowledgeArticleVector.EmbeddingIndexMethod)
            .HasOperators([DatabaseConstants.KnowledgeArticleVector.EmbeddingIndexOperator]);

        builder.HasOne(x => x.Article)
            .WithOne(x => x.Vector)
            .HasForeignKey<KnowledgeArticleVector>(x => x.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
