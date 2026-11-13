using Atlas.Infrastructure.Database.Constants;
using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class HardwareConfiguration : IEntityTypeConfiguration<Hardware>
{
    public void Configure(EntityTypeBuilder<Hardware> builder)
    {
        builder.ToTable(DatabaseConstants.Hardware.Table);

        builder.Property(x => x.Id)
            .HasColumnName(DatabaseConstants.Hardware.Id)
            .HasDefaultValueSql(DatabaseConstants.Defaults.GenRandomUuidSql);

        builder.Property(x => x.Name)
            .HasColumnName(DatabaseConstants.Hardware.Name)
            .HasMaxLength(DatabaseConstants.Hardware.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.EquipmentTypeId)
            .HasColumnName(DatabaseConstants.Hardware.EquipmentTypeId)
            .IsRequired();

        builder.Property(x => x.InventoryNumber)
            .HasColumnName(DatabaseConstants.Hardware.InventoryNumber)
            .HasMaxLength(DatabaseConstants.Hardware.InventoryNumberMaxLength);

        builder.Property(x => x.SerialNumber)
            .HasColumnName(DatabaseConstants.Hardware.SerialNumber)
            .HasMaxLength(DatabaseConstants.Hardware.SerialNumberMaxLength);

        builder.Property(x => x.RoomId)
            .HasColumnName(DatabaseConstants.Hardware.RoomId)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName(DatabaseConstants.Hardware.CreatedAt)
            .HasDefaultValueSql(DatabaseConstants.Defaults.NowSql);

        builder.Property(x => x.UpdatedAt)
            .HasColumnName(DatabaseConstants.Hardware.UpdatedAt)
            .HasDefaultValueSql(DatabaseConstants.Defaults.NowSql);

        builder.HasIndex(x => x.InventoryNumber)
            .IsUnique();

        builder.HasIndex(x => new { x.RoomId, x.EquipmentTypeId });

        builder.HasOne(x => x.Room)
            .WithMany(x => x.Hardware)
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.EquipmentType)
            .WithMany(x => x.Hardware)
            .HasForeignKey(x => x.EquipmentTypeId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
