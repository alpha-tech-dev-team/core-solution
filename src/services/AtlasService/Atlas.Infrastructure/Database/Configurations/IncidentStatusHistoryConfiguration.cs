using Atlas.Domain.Constants.AtlasDBConfiguration;
using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class IncidentStatusHistoryConfiguration : IEntityTypeConfiguration<IncidentStatusHistory>
{
    public void Configure(EntityTypeBuilder<IncidentStatusHistory> builder)
    {
        builder.ToTable(DBKeys.IncidentStatusHistory.Table, DBKeys.Shared.Schema);

        builder.HasKey(x => x.Id).HasName(DBKeys.IncidentStatusHistory.PrimaryKey);
        builder.Property(x => x.Id)
            .HasColumnName(DBKeys.Shared.Id)
            .ValueGeneratedOnAdd()
            .UseIdentityAlwaysColumn();

        builder.Property(x => x.IncidentId)
            .HasColumnName(DBKeys.IncidentStatusHistory.IncidentId)
            .IsRequired();

        builder.Property(x => x.OldStatus)
            .HasColumnName(DBKeys.IncidentStatusHistory.OldStatus)
            .HasMaxLength(DBKeys.Shared.MaxLength30);

        builder.Property(x => x.NewStatus)
            .HasColumnName(DBKeys.IncidentStatusHistory.NewStatus)
            .HasMaxLength(DBKeys.Shared.MaxLength30)
            .IsRequired();

        builder.Property(x => x.ChangedBy)
            .HasColumnName(DBKeys.IncidentStatusHistory.ChangedBy);

        builder.Property(x => x.Comment)
            .HasColumnName(DBKeys.IncidentStatusHistory.Comment);

        builder.Property(x => x.CreatedAt)
            .HasColumnName(DBKeys.Shared.CreatedAt)
            .HasColumnType(DBKeys.Shared.TimestampWithTimeZone)
            .HasDefaultValueSql(DBKeys.Shared.NowSql);

        builder.HasIndex(x => new { x.IncidentId, x.CreatedAt })
            .HasDatabaseName(DBKeys.IncidentStatusHistory.IncidentCreatedAtIndex);

        builder.HasOne(x => x.Incident)
            .WithMany(x => x.StatusHistory)
            .HasForeignKey(x => x.IncidentId)
            .HasConstraintName(DBKeys.IncidentStatusHistory.IncidentForeignKey)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ChangedByEmployee)
            .WithMany(x => x.StatusChanges)
            .HasForeignKey(x => x.ChangedBy)
            .HasConstraintName(DBKeys.IncidentStatusHistory.ChangedByForeignKey)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
