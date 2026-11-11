using Atlas.Infrastructure.Database.Constants;
using Atlas.Domain.Entities;
using Atlas.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable(DatabaseConstants.Employee.Table);

        builder.Property(x => x.Id)
            .HasColumnName(DatabaseConstants.Employee.Id);

        builder.Property(x => x.Role)
            .HasColumnName(DatabaseConstants.Employee.Role)
            .HasDefaultValue(EmployeeRole.Employee)
            .HasSentinel(EmployeeRole.Employee)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName(DatabaseConstants.Employee.CreatedAt)
            .HasDefaultValueSql(DatabaseConstants.Defaults.NowSql);

        builder.Property(x => x.UpdatedAt)
            .HasColumnName(DatabaseConstants.Employee.UpdatedAt)
            .HasDefaultValueSql(DatabaseConstants.Defaults.NowSql);
    }
}
