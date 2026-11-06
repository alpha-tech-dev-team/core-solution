using Atlas.Domain.Constants.AtlasDBConfiguration;
using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable(DBKeys.Employee.Table, DBKeys.Shared.Schema, table =>
            table.HasCheckConstraint(
                DBKeys.Employee.RoleCheckConstraint,
                DBKeys.Employee.RoleCheckSql));

        builder.HasKey(x => x.EmployeeId).HasName(DBKeys.Employee.PrimaryKey);
        builder.Property(x => x.EmployeeId)
            .HasColumnName(DBKeys.Employee.EmployeeId);

        builder.Property(x => x.TelegramId)
            .HasColumnName(DBKeys.Employee.TelegramId);

        builder.Property(x => x.Role)
            .HasColumnName(DBKeys.Employee.Role)
            .HasMaxLength(DBKeys.Shared.MaxLength20)
            .HasDefaultValue(DBKeys.Employee.RoleDefaultValue)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName(DBKeys.Shared.IsActive)
            .HasDefaultValue(true)
            .HasSentinel(true);

        builder.Property(x => x.CreatedAt)
            .HasColumnName(DBKeys.Shared.CreatedAt)
            .HasColumnType(DBKeys.Shared.TimestampWithTimeZone)
            .HasDefaultValueSql(DBKeys.Shared.NowSql);

        builder.Property(x => x.UpdatedAt)
            .HasColumnName(DBKeys.Shared.UpdatedAt)
            .HasColumnType(DBKeys.Shared.TimestampWithTimeZone)
            .HasDefaultValueSql(DBKeys.Shared.NowSql);

        builder.HasIndex(x => x.TelegramId)
            .IsUnique()
            .HasDatabaseName(DBKeys.Employee.TelegramIdUniqueIndex);
    }
}
