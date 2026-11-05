using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("incidents", "atlas", table =>
        {
            table.HasCheckConstraint(
                "chk_incident_status",
                "status IN ('created', 'in_progress', 'resolved', 'resolved_automatically', 'closed', 'cancelled')");
            table.HasCheckConstraint(
                "chk_incident_priority",
                "priority IN ('low', 'medium', 'high', 'critical')");
            table.HasCheckConstraint(
                "chk_incident_description",
                "length(trim(user_description)) > 0");
            table.HasCheckConstraint(
                "chk_incident_resolution",
                "status NOT IN ('resolved', 'resolved_automatically') OR (resolution IS NOT NULL AND length(trim(resolution)) > 0)");
            table.HasCheckConstraint(
                "chk_incident_assignee",
                "status <> 'in_progress' OR assigned_engineer_id IS NOT NULL");
            table.HasCheckConstraint(
                "chk_incident_asset",
                "hardware_id IS NULL OR software_id IS NULL");
        });

        builder.HasKey(x => x.Id).HasName("incidents_pkey");
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.TicketNumber)
            .HasColumnName("ticket_number")
            .ValueGeneratedOnAdd()
            .UseIdentityAlwaysColumn();

        builder.Property(x => x.EmployeeId)
            .HasColumnName("employee_id")
            .IsRequired();

        builder.Property(x => x.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();

        builder.Property(x => x.OfficeId)
            .HasColumnName("office_id");

        builder.Property(x => x.HardwareId)
            .HasColumnName("hardware_id");

        builder.Property(x => x.SoftwareId)
            .HasColumnName("software_id");

        builder.Property(x => x.UserDescription)
            .HasColumnName("user_description")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .HasDefaultValue("created")
            .IsRequired();

        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .HasMaxLength(20)
            .HasDefaultValue("medium")
            .IsRequired();

        builder.Property(x => x.AssignedEngineerId)
            .HasColumnName("assigned_engineer_id");

        builder.Property(x => x.ArticleId)
            .HasColumnName("article_id");

        builder.Property(x => x.Resolution)
            .HasColumnName("resolution");

        builder.Property(x => x.ResolvedAt)
            .HasColumnName("resolved_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        builder.HasIndex(x => x.TicketNumber)
            .IsUnique()
            .HasDatabaseName("incidents_ticket_number_key");

        builder.HasIndex(x => x.AssignedEngineerId)
            .HasDatabaseName("idx_incidents_assigned_engineer");

        builder.HasIndex(x => x.CategoryId)
            .HasDatabaseName("idx_incidents_category");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("idx_incidents_created_at");

        builder.HasIndex(x => x.EmployeeId)
            .HasDatabaseName("idx_incidents_employee");

        builder.HasIndex(x => new { x.Status, x.Priority })
            .HasDatabaseName("idx_incidents_status_priority");

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.EmployeeIncidents)
            .HasForeignKey(x => x.EmployeeId)
            .HasConstraintName("incidents_employee_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.AssignedEngineer)
            .WithMany(x => x.AssignedIncidents)
            .HasForeignKey(x => x.AssignedEngineerId)
            .HasConstraintName("incidents_assigned_engineer_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Incidents)
            .HasForeignKey(x => x.CategoryId)
            .HasConstraintName("incidents_category_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Office)
            .WithMany(x => x.Incidents)
            .HasForeignKey(x => x.OfficeId)
            .HasConstraintName("incidents_office_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Hardware)
            .WithMany(x => x.Incidents)
            .HasForeignKey(x => x.HardwareId)
            .HasConstraintName("incidents_hardware_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Software)
            .WithMany(x => x.Incidents)
            .HasForeignKey(x => x.SoftwareId)
            .HasConstraintName("incidents_software_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Article)
            .WithMany()
            .HasForeignKey(x => x.ArticleId)
            .HasConstraintName("fk_incidents_article")
            .OnDelete(DeleteBehavior.SetNull);
    }
}
