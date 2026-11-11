using Atlas.Infrastructure.Database.Constants;
using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class IncidentFeedbackConfiguration : IEntityTypeConfiguration<IncidentFeedback>
{
    public void Configure(EntityTypeBuilder<IncidentFeedback> builder)
    {
        builder.ToTable(DatabaseConstants.IncidentFeedback.Table);

        builder.Property(x => x.Id)
            .HasColumnName(DatabaseConstants.IncidentFeedback.Id)
            .HasDefaultValueSql(DatabaseConstants.Defaults.GenRandomUuidSql);

        builder.Property(x => x.IncidentId)
            .HasColumnName(DatabaseConstants.IncidentFeedback.IncidentId)
            .IsRequired();

        builder.Property(x => x.ArticleId)
            .HasColumnName(DatabaseConstants.IncidentFeedback.ArticleId)
            .IsRequired();

        builder.Property(x => x.WasHelpful)
            .HasColumnName(DatabaseConstants.IncidentFeedback.WasHelpful)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName(DatabaseConstants.IncidentFeedback.CreatedAt)
            .HasDefaultValueSql(DatabaseConstants.Defaults.NowSql);

        builder.HasIndex(x => new { x.IncidentId, x.ArticleId })
            .IsUnique();

        builder.HasIndex(x => x.ArticleId);

        builder.HasOne(x => x.Incident)
            .WithMany(x => x.Feedback)
            .HasForeignKey(x => x.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Article)
            .WithMany(x => x.Feedback)
            .HasForeignKey(x => x.ArticleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
