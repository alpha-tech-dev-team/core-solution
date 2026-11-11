using Atlas.Infrastructure.Database.Constants;
using Atlas.Domain.Entities;
using Atlas.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable(DatabaseConstants.Incident.Table);

        builder.Property(x => x.Id)
            .HasColumnName(DatabaseConstants.Incident.Id)
            .HasDefaultValueSql(DatabaseConstants.Defaults.GenRandomUuidSql);

        builder.Property(x => x.TicketNumber)
            .HasColumnName(DatabaseConstants.Incident.TicketNumber)
            .ValueGeneratedOnAdd()
            .UseIdentityAlwaysColumn();

        builder.Property(x => x.EmployeeId)
            .HasColumnName(DatabaseConstants.Incident.EmployeeId)
            .IsRequired();

        builder.Property(x => x.CategoryId)
            .HasColumnName(DatabaseConstants.Incident.CategoryId)
            .IsRequired();

        builder.Property(x => x.RoomId)
            .HasColumnName(DatabaseConstants.Incident.RoomId);

        builder.Property(x => x.HardwareId)
            .HasColumnName(DatabaseConstants.Incident.HardwareId);

        builder.Property(x => x.SoftwareId)
            .HasColumnName(DatabaseConstants.Incident.SoftwareId);

        builder.Property(x => x.UserDescription)
            .HasColumnName(DatabaseConstants.Incident.UserDescription)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName(DatabaseConstants.Incident.Status)
            .HasDefaultValue(IncidentStatus.Created)
            .IsRequired();

        builder.Property(x => x.Priority)
            .HasColumnName(DatabaseConstants.Incident.Priority)
            .HasDefaultValue(IncidentPriority.Medium)
            .HasSentinel(IncidentPriority.Medium)
            .IsRequired();

        builder.Property(x => x.AssignedEngineerId)
            .HasColumnName(DatabaseConstants.Incident.AssignedEngineerId);

        builder.Property(x => x.ArticleId)
            .HasColumnName(DatabaseConstants.Incident.ArticleId);

        builder.Property(x => x.Resolution)
            .HasColumnName(DatabaseConstants.Incident.Resolution);

        builder.Property(x => x.ResolvedAt)
            .HasColumnName(DatabaseConstants.Incident.ResolvedAt);

        builder.Property(x => x.CreatedAt)
            .HasColumnName(DatabaseConstants.Incident.CreatedAt)
            .HasDefaultValueSql(DatabaseConstants.Defaults.NowSql);

        builder.Property(x => x.UpdatedAt)
            .HasColumnName(DatabaseConstants.Incident.UpdatedAt)
            .HasDefaultValueSql(DatabaseConstants.Defaults.NowSql);

        builder.HasIndex(x => x.AssignedEngineerId);
        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => new { x.Status, x.Priority });

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.EmployeeIncidents)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.AssignedEngineer)
            .WithMany(x => x.AssignedIncidents)
            .HasForeignKey(x => x.AssignedEngineerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Incidents)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Room)
            .WithMany(x => x.Incidents)
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Hardware)
            .WithMany(x => x.Incidents)
            .HasForeignKey(x => x.HardwareId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Software)
            .WithMany(x => x.Incidents)
            .HasForeignKey(x => x.SoftwareId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Article)
            .WithMany()
            .HasForeignKey(x => x.ArticleId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
