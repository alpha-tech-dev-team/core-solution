using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class HardwareConfiguration : IEntityTypeConfiguration<Hardware>
{
    public void Configure(EntityTypeBuilder<Hardware> builder)
    {
        builder.ToTable("hardware", "atlas");

        builder.HasKey(x => x.Id).HasName("hardware_pkey");
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.InventoryNumber)
            .HasColumnName("inventory_number")
            .HasMaxLength(100);

        builder.Property(x => x.SerialNumber)
            .HasColumnName("serial_number")
            .HasMaxLength(255);

        builder.Property(x => x.OfficeId)
            .HasColumnName("office_id");

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

        builder.HasIndex(x => x.InventoryNumber)
            .IsUnique()
            .HasDatabaseName("hardware_inventory_number_key");

        builder.HasIndex(x => new { x.OfficeId, x.Type })
            .HasDatabaseName("idx_hardware_office_type");

        builder.HasOne(x => x.Office)
            .WithMany(x => x.Hardware)
            .HasForeignKey(x => x.OfficeId)
            .HasConstraintName("hardware_office_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
