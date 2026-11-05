using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class EmployeeAccessConfiguration : IEntityTypeConfiguration<EmployeeAccess>
{
    public void Configure(EntityTypeBuilder<EmployeeAccess> builder)
    {
        builder.ToTable("employee_access", "atlas", table =>
            table.HasCheckConstraint(
                "chk_employee_access_role",
                "role IN ('employee', 'engineer', 'admin')"));

        builder.HasKey(x => x.EmployeeId).HasName("employee_access_pkey");
        builder.Property(x => x.EmployeeId)
            .HasColumnName("employee_id");

        builder.Property(x => x.TelegramId)
            .HasColumnName("telegram_id");

        builder.Property(x => x.Role)
            .HasColumnName("role")
            .HasMaxLength(20)
            .HasDefaultValue("employee")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        builder.HasIndex(x => x.TelegramId)
            .IsUnique()
            .HasDatabaseName("employee_access_telegram_id_key");
    }
}
