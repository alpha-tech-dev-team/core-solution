using Atlas.Infrastructure.Database.Constants;
using Atlas.Domain.Entities;
using Atlas.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class KnowledgeArticleConfiguration : IEntityTypeConfiguration<KnowledgeArticle>
{
    public void Configure(EntityTypeBuilder<KnowledgeArticle> builder)
    {
        builder.ToTable(DatabaseConstants.KnowledgeArticle.Table);

        builder.Property(x => x.Id)
            .HasColumnName(DatabaseConstants.KnowledgeArticle.Id)
            .HasDefaultValueSql(DatabaseConstants.Defaults.GenRandomUuidSql);

        builder.Property(x => x.CategoryId)
            .HasColumnName(DatabaseConstants.KnowledgeArticle.CategoryId);

        builder.Property(x => x.Title)
            .HasColumnName(DatabaseConstants.KnowledgeArticle.Title)
            .HasMaxLength(DatabaseConstants.KnowledgeArticle.TitleMaxLength)
            .IsRequired();

        builder.Property(x => x.Symptoms)
            .HasColumnName(DatabaseConstants.KnowledgeArticle.Symptoms);

        builder.Property(x => x.Solution)
            .HasColumnName(DatabaseConstants.KnowledgeArticle.Solution)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName(DatabaseConstants.KnowledgeArticle.Status)
            .HasDefaultValue(KnowledgeArticleStatus.Draft)
            .HasSentinel(KnowledgeArticleStatus.Draft)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasColumnName(DatabaseConstants.KnowledgeArticle.CreatedBy);

        builder.Property(x => x.SourceIncidentId)
            .HasColumnName(DatabaseConstants.KnowledgeArticle.SourceIncidentId);

        builder.Property(x => x.CreatedAt)
            .HasColumnName(DatabaseConstants.KnowledgeArticle.CreatedAt)
            .HasDefaultValueSql(DatabaseConstants.Defaults.NowSql);

        builder.Property(x => x.UpdatedAt)
            .HasColumnName(DatabaseConstants.KnowledgeArticle.UpdatedAt)
            .HasDefaultValueSql(DatabaseConstants.Defaults.NowSql);

        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.SourceIncidentId)
            .IsUnique();

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Articles)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.CreatedByEmployee)
            .WithMany(x => x.CreatedArticles)
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.SourceIncident)
            .WithMany()
            .HasForeignKey(x => x.SourceIncidentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
