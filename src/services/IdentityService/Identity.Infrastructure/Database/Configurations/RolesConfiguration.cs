using Identity.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Database.Configurations;

public class RolesConfiguration : IEntityTypeConfiguration<Roles>
{
    public void Configure(EntityTypeBuilder<Roles> builder)
    {
        builder.ToTable(DatabaseConstants.Roles.Table);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName(DatabaseConstants.Roles.Id);

        builder.Property(x => x.Name)
            .HasColumnName(DatabaseConstants.Roles.Name)
            .HasMaxLength(DatabaseConstants.Roles.NameMaxLength)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
