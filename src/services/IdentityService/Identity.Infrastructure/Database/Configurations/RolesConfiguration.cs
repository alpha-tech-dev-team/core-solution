using Identity.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Database.Configurations;

public class RolesConfiguration : IEntityTypeConfiguration<Roles>
{
    public void Configure(EntityTypeBuilder<Roles> builder)
    {
        builder.ToTable(DatabaseConstants.Tables.Roles);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName(DatabaseConstants.Columns.Id);

        builder.Property(x => x.Name)
            .HasColumnName(DatabaseConstants.Columns.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
