using Atlas.Infrastructure.Database.Constants;
using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pgvector;
using Pgvector.EntityFrameworkCore;

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

        embedding.Metadata.SetValueComparer(new ValueComparer<float[]>(
            (left, right) => ReferenceEquals(left, right)
                || (left != null && right != null && left.SequenceEqual(right)),
            values => values == null
                ? 0
                : values.Aggregate(0, (hash, value) => HashCode.Combine(hash, value.GetHashCode())),
            values => values == null ? null! : values.ToArray()));

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
