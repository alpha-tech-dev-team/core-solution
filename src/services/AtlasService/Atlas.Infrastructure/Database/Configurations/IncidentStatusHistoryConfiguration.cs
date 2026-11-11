using Atlas.Infrastructure.Database.Constants;
using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class IncidentStatusHistoryConfiguration : IEntityTypeConfiguration<IncidentStatusHistory>
{
    public void Configure(EntityTypeBuilder<IncidentStatusHistory> builder)
    {
        builder.ToTable(DatabaseConstants.IncidentStatusHistory.Table);

        builder.Property(x => x.Id)
            .HasColumnName(DatabaseConstants.IncidentStatusHistory.Id)
            .HasDefaultValueSql(DatabaseConstants.Defaults.GenRandomUuidSql);

        builder.Property(x => x.IncidentId)
            .HasColumnName(DatabaseConstants.IncidentStatusHistory.IncidentId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName(DatabaseConstants.IncidentStatusHistory.Status)
            .IsRequired();

        builder.Property(x => x.LastChangedBy)
            .HasColumnName(DatabaseConstants.IncidentStatusHistory.LastChangedBy);

        builder.Property(x => x.Comment)
            .HasColumnName(DatabaseConstants.IncidentStatusHistory.Comment);

        builder.Property(x => x.CreatedAt)
            .HasColumnName(DatabaseConstants.IncidentStatusHistory.CreatedAt)
            .HasDefaultValueSql(DatabaseConstants.Defaults.NowSql);

        builder.HasIndex(x => new { x.IncidentId, x.CreatedAt });

        builder.HasOne(x => x.Incident)
            .WithMany(x => x.StatusHistory)
            .HasForeignKey(x => x.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.LastChangedByEmployee)
            .WithMany(x => x.StatusChanges)
            .HasForeignKey(x => x.LastChangedBy)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
