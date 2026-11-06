using Atlas.Domain.Constants.AtlasDBConfiguration;
using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pgvector.EntityFrameworkCore;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class KnowledgeArticleVectorConfiguration : IEntityTypeConfiguration<KnowledgeArticleVector>
{
    public void Configure(EntityTypeBuilder<KnowledgeArticleVector> builder)
    {
        builder.ToTable(DBKeys.KnowledgeArticleVector.Table, DBKeys.Shared.Schema);

        builder.HasKey(x => x.ArticleId).HasName(DBKeys.KnowledgeArticleVector.PrimaryKey);
        builder.Property(x => x.ArticleId)
            .HasColumnName(DBKeys.KnowledgeArticleVector.ArticleId);

        builder.Property(x => x.Embedding)
            .HasColumnName(DBKeys.KnowledgeArticleVector.Embedding)
            .HasColumnType(DBKeys.KnowledgeArticleVector.EmbeddingColumnType)
            .IsRequired();

        builder.Property(x => x.EmbeddingModel)
            .HasColumnName(DBKeys.KnowledgeArticleVector.EmbeddingModel)
            .HasMaxLength(DBKeys.Shared.MaxLength100)
            .IsRequired();

        builder.Property(x => x.EmbeddedText)
            .HasColumnName(DBKeys.KnowledgeArticleVector.EmbeddedText)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName(DBKeys.Shared.UpdatedAt)
            .HasColumnType(DBKeys.Shared.TimestampWithTimeZone)
            .HasDefaultValueSql(DBKeys.Shared.NowSql);

        builder.HasIndex(x => x.Embedding)
            .HasMethod(DBKeys.KnowledgeArticleVector.EmbeddingIndexMethod)
            .HasOperators(DBKeys.KnowledgeArticleVector.EmbeddingIndexOperators)
            .HasDatabaseName(DBKeys.KnowledgeArticleVector.EmbeddingIndex);

        builder.HasOne(x => x.Article)
            .WithOne(x => x.Vector)
            .HasForeignKey<KnowledgeArticleVector>(x => x.ArticleId)
            .HasConstraintName(DBKeys.KnowledgeArticleVector.ArticleForeignKey)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
