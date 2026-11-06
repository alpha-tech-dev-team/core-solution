using Atlas.Domain.Constants.AtlasDBConfiguration;
using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class IncidentFeedbackConfiguration : IEntityTypeConfiguration<IncidentFeedback>
{
    public void Configure(EntityTypeBuilder<IncidentFeedback> builder)
    {
        builder.ToTable(DBKeys.IncidentFeedback.Table, DBKeys.Shared.Schema);

        builder.HasKey(x => x.Id).HasName(DBKeys.IncidentFeedback.PrimaryKey);
        builder.Property(x => x.Id)
            .HasColumnName(DBKeys.Shared.Id)
            .HasDefaultValueSql(DBKeys.Shared.GenRandomUuidSql);

        builder.Property(x => x.IncidentId)
            .HasColumnName(DBKeys.IncidentFeedback.IncidentId)
            .IsRequired();

        builder.Property(x => x.ArticleId)
            .HasColumnName(DBKeys.IncidentFeedback.ArticleId)
            .IsRequired();

        builder.Property(x => x.WasHelpful)
            .HasColumnName(DBKeys.IncidentFeedback.WasHelpful)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName(DBKeys.Shared.CreatedAt)
            .HasColumnType(DBKeys.Shared.TimestampWithTimeZone)
            .HasDefaultValueSql(DBKeys.Shared.NowSql);

        builder.HasIndex(x => new { x.IncidentId, x.ArticleId })
            .IsUnique()
            .HasDatabaseName(DBKeys.IncidentFeedback.IncidentArticleUniqueIndex);

        builder.HasIndex(x => x.ArticleId)
            .HasDatabaseName(DBKeys.IncidentFeedback.ArticleIndex);

        builder.HasOne(x => x.Incident)
            .WithMany(x => x.Feedback)
            .HasForeignKey(x => x.IncidentId)
            .HasConstraintName(DBKeys.IncidentFeedback.IncidentForeignKey)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Article)
            .WithMany(x => x.Feedback)
            .HasForeignKey(x => x.ArticleId)
            .HasConstraintName(DBKeys.IncidentFeedback.ArticleForeignKey)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
