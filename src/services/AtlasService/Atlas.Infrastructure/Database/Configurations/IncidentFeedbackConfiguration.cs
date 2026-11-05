using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class IncidentFeedbackConfiguration : IEntityTypeConfiguration<IncidentFeedback>
{
    public void Configure(EntityTypeBuilder<IncidentFeedback> builder)
    {
        builder.ToTable("incident_feedback", "atlas");

        builder.HasKey(x => x.Id).HasName("incident_feedback_pkey");
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.IncidentId)
            .HasColumnName("incident_id")
            .IsRequired();

        builder.Property(x => x.ArticleId)
            .HasColumnName("article_id")
            .IsRequired();

        builder.Property(x => x.WasHelpful)
            .HasColumnName("was_helpful")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        builder.HasIndex(x => new { x.IncidentId, x.ArticleId })
            .IsUnique()
            .HasDatabaseName("uq_incident_feedback");

        builder.HasIndex(x => x.ArticleId)
            .HasDatabaseName("idx_incident_feedback_article");

        builder.HasOne(x => x.Incident)
            .WithMany(x => x.Feedback)
            .HasForeignKey(x => x.IncidentId)
            .HasConstraintName("incident_feedback_incident_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Article)
            .WithMany(x => x.Feedback)
            .HasForeignKey(x => x.ArticleId)
            .HasConstraintName("incident_feedback_article_id_fkey")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
