using Atlas.Domain.Constants.AtlasDBConfiguration;
using Atlas.Domain.Entities;
using Atlas.Domain.Enums;
using Atlas.Infrastructure.Database.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable(DBKeys.Incident.Table, DBKeys.Shared.Schema, table =>
        {
            table.HasCheckConstraint(
                DBKeys.Incident.StatusCheckConstraint,
                DBKeys.Incident.StatusCheckSql);
            table.HasCheckConstraint(
                DBKeys.Incident.PriorityCheckConstraint,
                DBKeys.Incident.PriorityCheckSql);
            table.HasCheckConstraint(
                DBKeys.Incident.DescriptionCheckConstraint,
                DBKeys.Incident.DescriptionCheckSql);
            table.HasCheckConstraint(
                DBKeys.Incident.ResolutionCheckConstraint,
                DBKeys.Incident.ResolutionCheckSql);
            table.HasCheckConstraint(
                DBKeys.Incident.AssigneeCheckConstraint,
                DBKeys.Incident.AssigneeCheckSql);
            table.HasCheckConstraint(
                DBKeys.Incident.AssetCheckConstraint,
                DBKeys.Incident.AssetCheckSql);
        });

        builder.HasKey(x => x.Id).HasName(DBKeys.Incident.PrimaryKey);
        builder.Property(x => x.Id)
            .HasColumnName(DBKeys.Shared.Id)
            .HasDefaultValueSql(DBKeys.Shared.GenRandomUuidSql);

        builder.Property(x => x.TicketNumber)
            .HasColumnName(DBKeys.Incident.TicketNumber)
            .ValueGeneratedOnAdd()
            .UseIdentityAlwaysColumn();

        builder.Property(x => x.EmployeeId)
            .HasColumnName(DBKeys.Incident.EmployeeId)
            .IsRequired();

        builder.Property(x => x.CategoryId)
            .HasColumnName(DBKeys.Incident.CategoryId)
            .IsRequired();

        builder.Property(x => x.OfficeId)
            .HasColumnName(DBKeys.Incident.OfficeId);

        builder.Property(x => x.HardwareId)
            .HasColumnName(DBKeys.Incident.HardwareId);

        builder.Property(x => x.SoftwareId)
            .HasColumnName(DBKeys.Incident.SoftwareId);

        builder.Property(x => x.UserDescription)
            .HasColumnName(DBKeys.Incident.UserDescription)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName(DBKeys.Incident.Status)
            .HasConversion<IncidentStatusConverter>()
            .HasMaxLength(DBKeys.Shared.MaxLength30)
            .HasDefaultValue(IncidentStatus.Created)
            .IsRequired();

        builder.Property(x => x.Priority)
            .HasColumnName(DBKeys.Incident.Priority)
            .HasMaxLength(DBKeys.Shared.MaxLength20)
            .HasDefaultValue(DBKeys.Incident.PriorityDefaultValue)
            .IsRequired();

        builder.Property(x => x.AssignedEngineerId)
            .HasColumnName(DBKeys.Incident.AssignedEngineerId);

        builder.Property(x => x.ArticleId)
            .HasColumnName(DBKeys.Incident.ArticleId);

        builder.Property(x => x.Resolution)
            .HasColumnName(DBKeys.Incident.Resolution);

        builder.Property(x => x.ResolvedAt)
            .HasColumnName(DBKeys.Incident.ResolvedAt)
            .HasColumnType(DBKeys.Shared.TimestampWithTimeZone);

        builder.Property(x => x.CreatedAt)
            .HasColumnName(DBKeys.Shared.CreatedAt)
            .HasColumnType(DBKeys.Shared.TimestampWithTimeZone)
            .HasDefaultValueSql(DBKeys.Shared.NowSql);

        builder.Property(x => x.UpdatedAt)
            .HasColumnName(DBKeys.Shared.UpdatedAt)
            .HasColumnType(DBKeys.Shared.TimestampWithTimeZone)
            .HasDefaultValueSql(DBKeys.Shared.NowSql);

        builder.HasIndex(x => x.TicketNumber)
            .IsUnique()
            .HasDatabaseName(DBKeys.Incident.TicketNumberUniqueIndex);

        builder.HasIndex(x => x.AssignedEngineerId)
            .HasDatabaseName(DBKeys.Incident.AssignedEngineerIndex);

        builder.HasIndex(x => x.CategoryId)
            .HasDatabaseName(DBKeys.Incident.CategoryIndex);

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName(DBKeys.Incident.CreatedAtIndex);

        builder.HasIndex(x => x.EmployeeId)
            .HasDatabaseName(DBKeys.Incident.EmployeeIndex);

        builder.HasIndex(x => new { x.Status, x.Priority })
            .HasDatabaseName(DBKeys.Incident.StatusPriorityIndex);

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.EmployeeIncidents)
            .HasForeignKey(x => x.EmployeeId)
            .HasConstraintName(DBKeys.Incident.EmployeeForeignKey)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.AssignedEngineer)
            .WithMany(x => x.AssignedIncidents)
            .HasForeignKey(x => x.AssignedEngineerId)
            .HasConstraintName(DBKeys.Incident.AssignedEngineerForeignKey)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Incidents)
            .HasForeignKey(x => x.CategoryId)
            .HasConstraintName(DBKeys.Incident.CategoryForeignKey)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Office)
            .WithMany(x => x.Incidents)
            .HasForeignKey(x => x.OfficeId)
            .HasConstraintName(DBKeys.Incident.OfficeForeignKey)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Hardware)
            .WithMany(x => x.Incidents)
            .HasForeignKey(x => x.HardwareId)
            .HasConstraintName(DBKeys.Incident.HardwareForeignKey)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Software)
            .WithMany(x => x.Incidents)
            .HasForeignKey(x => x.SoftwareId)
            .HasConstraintName(DBKeys.Incident.SoftwareForeignKey)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Article)
            .WithMany()
            .HasForeignKey(x => x.ArticleId)
            .HasConstraintName(DBKeys.Incident.ArticleForeignKey)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
