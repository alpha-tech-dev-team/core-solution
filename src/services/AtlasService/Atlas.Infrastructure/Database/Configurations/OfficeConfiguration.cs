using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class OfficeConfiguration : IEntityTypeConfiguration<Office>
{
    public void Configure(EntityTypeBuilder<Office> builder)
    {
        builder.ToTable("offices", "atlas");

        builder.HasKey(x => x.Id).HasName("offices_pkey");
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.RoomNumber)
            .HasColumnName("room_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description");

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

        builder.HasIndex(x => x.RoomNumber)
            .IsUnique()
            .HasDatabaseName("offices_room_number_key");
    }
}
