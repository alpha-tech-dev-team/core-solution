using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class IncidentStatusHistoryConfiguration : IEntityTypeConfiguration<IncidentStatusHistory>
{
    public void Configure(EntityTypeBuilder<IncidentStatusHistory> builder)
    {
        builder.ToTable("incident_status_history", "atlas");

        builder.HasKey(x => x.Id).HasName("incident_status_history_pkey");
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd()
            .UseIdentityAlwaysColumn();

        builder.Property(x => x.IncidentId)
            .HasColumnName("incident_id")
            .IsRequired();

        builder.Property(x => x.OldStatus)
            .HasColumnName("old_status")
            .HasMaxLength(30);

        builder.Property(x => x.NewStatus)
            .HasColumnName("new_status")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.ChangedBy)
            .HasColumnName("changed_by");

        builder.Property(x => x.Comment)
            .HasColumnName("comment");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        builder.HasIndex(x => new { x.IncidentId, x.CreatedAt })
            .HasDatabaseName("idx_incident_status_history_incident");

        builder.HasOne(x => x.Incident)
            .WithMany(x => x.StatusHistory)
            .HasForeignKey(x => x.IncidentId)
            .HasConstraintName("incident_status_history_incident_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ChangedByEmployee)
            .WithMany(x => x.StatusChanges)
            .HasForeignKey(x => x.ChangedBy)
            .HasConstraintName("incident_status_history_changed_by_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
