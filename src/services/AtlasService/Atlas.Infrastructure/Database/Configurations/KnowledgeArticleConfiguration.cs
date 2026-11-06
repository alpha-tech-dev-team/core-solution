using Atlas.Domain.Constants.AtlasDBConfiguration;
using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class KnowledgeArticleConfiguration : IEntityTypeConfiguration<KnowledgeArticle>
{
    public void Configure(EntityTypeBuilder<KnowledgeArticle> builder)
    {
        builder.ToTable(DBKeys.KnowledgeArticle.Table, DBKeys.Shared.Schema, table =>
        {
            table.HasCheckConstraint(
                DBKeys.KnowledgeArticle.StatusCheckConstraint,
                DBKeys.KnowledgeArticle.StatusCheckSql);
            table.HasCheckConstraint(
                DBKeys.KnowledgeArticle.TitleCheckConstraint,
                DBKeys.KnowledgeArticle.TitleCheckSql);
            table.HasCheckConstraint(
                DBKeys.KnowledgeArticle.SolutionCheckConstraint,
                DBKeys.KnowledgeArticle.SolutionCheckSql);
        });

        builder.HasKey(x => x.Id).HasName(DBKeys.KnowledgeArticle.PrimaryKey);
        builder.Property(x => x.Id)
            .HasColumnName(DBKeys.Shared.Id)
            .HasDefaultValueSql(DBKeys.Shared.GenRandomUuidSql);

        builder.Property(x => x.CategoryId)
            .HasColumnName(DBKeys.KnowledgeArticle.CategoryId);

        builder.Property(x => x.Title)
            .HasColumnName(DBKeys.KnowledgeArticle.Title)
            .HasMaxLength(DBKeys.Shared.MaxLength255)
            .IsRequired();

        builder.Property(x => x.Symptoms)
            .HasColumnName(DBKeys.KnowledgeArticle.Symptoms);

        builder.Property(x => x.Solution)
            .HasColumnName(DBKeys.KnowledgeArticle.Solution)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName(DBKeys.KnowledgeArticle.Status)
            .HasMaxLength(DBKeys.Shared.MaxLength20)
            .HasDefaultValue(DBKeys.KnowledgeArticle.StatusDefaultValue)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasColumnName(DBKeys.KnowledgeArticle.CreatedBy);

        builder.Property(x => x.SourceIncidentId)
            .HasColumnName(DBKeys.KnowledgeArticle.SourceIncidentId);

        builder.Property(x => x.CreatedAt)
            .HasColumnName(DBKeys.Shared.CreatedAt)
            .HasColumnType(DBKeys.Shared.TimestampWithTimeZone)
            .HasDefaultValueSql(DBKeys.Shared.NowSql);

        builder.Property(x => x.UpdatedAt)
            .HasColumnName(DBKeys.Shared.UpdatedAt)
            .HasColumnType(DBKeys.Shared.TimestampWithTimeZone)
            .HasDefaultValueSql(DBKeys.Shared.NowSql);

        builder.HasIndex(x => x.CategoryId)
            .HasDatabaseName(DBKeys.KnowledgeArticle.CategoryIndex);

        builder.HasIndex(x => x.Status)
            .HasDatabaseName(DBKeys.KnowledgeArticle.StatusIndex);

        builder.HasIndex(x => x.SourceIncidentId)
            .IsUnique()
            .HasDatabaseName(DBKeys.KnowledgeArticle.SourceIncidentUniqueIndex);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Articles)
            .HasForeignKey(x => x.CategoryId)
            .HasConstraintName(DBKeys.KnowledgeArticle.CategoryForeignKey)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.CreatedByEmployee)
            .WithMany(x => x.CreatedArticles)
            .HasForeignKey(x => x.CreatedBy)
            .HasConstraintName(DBKeys.KnowledgeArticle.CreatedByForeignKey)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.SourceIncident)
            .WithMany()
            .HasForeignKey(x => x.SourceIncidentId)
            .HasConstraintName(DBKeys.KnowledgeArticle.SourceIncidentForeignKey)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
