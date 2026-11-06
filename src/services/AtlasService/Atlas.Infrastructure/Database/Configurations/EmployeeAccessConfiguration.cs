using Atlas.Domain.Constants.AtlasDBConfiguration;
using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class EmployeeAccessConfiguration : IEntityTypeConfiguration<EmployeeAccess>
{
    public void Configure(EntityTypeBuilder<EmployeeAccess> builder)
    {
        builder.ToTable(DBKeys.EmployeeAccess.Table, DBKeys.Shared.Schema, table =>
            table.HasCheckConstraint(
                DBKeys.EmployeeAccess.RoleCheckConstraint,
                DBKeys.EmployeeAccess.RoleCheckSql));

        builder.HasKey(x => x.EmployeeId).HasName(DBKeys.EmployeeAccess.PrimaryKey);
        builder.Property(x => x.EmployeeId)
            .HasColumnName(DBKeys.EmployeeAccess.EmployeeId);

        builder.Property(x => x.TelegramId)
            .HasColumnName(DBKeys.EmployeeAccess.TelegramId);

        builder.Property(x => x.Role)
            .HasColumnName(DBKeys.EmployeeAccess.Role)
            .HasMaxLength(DBKeys.Shared.MaxLength20)
            .HasDefaultValue(DBKeys.EmployeeAccess.RoleDefaultValue)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName(DBKeys.Shared.IsActive)
            .HasDefaultValue(true);

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
            .HasDatabaseName(DBKeys.EmployeeAccess.TelegramIdUniqueIndex);
    }
}
