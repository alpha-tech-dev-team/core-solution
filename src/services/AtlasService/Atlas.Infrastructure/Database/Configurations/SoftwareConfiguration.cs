using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class SoftwareConfiguration : IEntityTypeConfiguration<Software>
{
    public void Configure(EntityTypeBuilder<Software> builder)
    {
        builder.ToTable("software", "atlas");

        builder.HasKey(x => x.Id).HasName("software_pkey");
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Version)
            .HasColumnName("version")
            .HasMaxLength(100);

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

        builder.HasIndex(x => new { x.Name, x.Version })
            .IsUnique()
            .HasDatabaseName("uq_software_name_version");
    }
}
