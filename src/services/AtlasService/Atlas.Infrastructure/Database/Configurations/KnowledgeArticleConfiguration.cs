using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class KnowledgeArticleConfiguration : IEntityTypeConfiguration<KnowledgeArticle>
{
    public void Configure(EntityTypeBuilder<KnowledgeArticle> builder)
    {
        builder.ToTable("knowledge_articles", "atlas", table =>
        {
            table.HasCheckConstraint(
                "chk_article_status",
                "status IN ('draft', 'published', 'archived')");
            table.HasCheckConstraint(
                "chk_article_title",
                "length(trim(title)) > 0");
            table.HasCheckConstraint(
                "chk_article_solution",
                "length(trim(solution)) > 0");
        });

        builder.HasKey(x => x.Id).HasName("knowledge_articles_pkey");
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.CategoryId)
            .HasColumnName("category_id");

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Symptoms)
            .HasColumnName("symptoms");

        builder.Property(x => x.Solution)
            .HasColumnName("solution")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasDefaultValue("draft")
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(x => x.SourceIncidentId)
            .HasColumnName("source_incident_id");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        builder.HasIndex(x => x.CategoryId)
            .HasDatabaseName("idx_knowledge_articles_category");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("idx_knowledge_articles_status");

        builder.HasIndex(x => x.SourceIncidentId)
            .IsUnique()
            .HasDatabaseName("knowledge_articles_source_incident_id_key");

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Articles)
            .HasForeignKey(x => x.CategoryId)
            .HasConstraintName("knowledge_articles_category_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.CreatedByEmployee)
            .WithMany(x => x.CreatedArticles)
            .HasForeignKey(x => x.CreatedBy)
            .HasConstraintName("knowledge_articles_created_by_fkey")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.SourceIncident)
            .WithMany()
            .HasForeignKey(x => x.SourceIncidentId)
            .HasConstraintName("knowledge_articles_source_incident_id_fkey")
            .OnDelete(DeleteBehavior.SetNull);
    }
}
