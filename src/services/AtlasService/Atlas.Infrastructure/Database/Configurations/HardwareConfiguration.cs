using Atlas.Domain.Constants.AtlasDBConfiguration;
using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class HardwareConfiguration : IEntityTypeConfiguration<Hardware>
{
    public void Configure(EntityTypeBuilder<Hardware> builder)
    {
        builder.ToTable(DBKeys.Hardware.Table, DBKeys.Shared.Schema);

        builder.HasKey(x => x.Id).HasName(DBKeys.Hardware.PrimaryKey);
        builder.Property(x => x.Id)
            .HasColumnName(DBKeys.Shared.Id)
            .HasDefaultValueSql(DBKeys.Shared.GenRandomUuidSql);

        builder.Property(x => x.Name)
            .HasColumnName(DBKeys.Hardware.Name)
            .HasMaxLength(DBKeys.Shared.MaxLength255)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName(DBKeys.Hardware.Type)
            .HasMaxLength(DBKeys.Shared.MaxLength100)
            .IsRequired();

        builder.Property(x => x.InventoryNumber)
            .HasColumnName(DBKeys.Hardware.InventoryNumber)
            .HasMaxLength(DBKeys.Shared.MaxLength100);

        builder.Property(x => x.SerialNumber)
            .HasColumnName(DBKeys.Hardware.SerialNumber)
            .HasMaxLength(DBKeys.Shared.MaxLength255);

        builder.Property(x => x.CabinetId)
            .HasColumnName(DBKeys.Hardware.CabinetId)
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

        builder.HasIndex(x => x.InventoryNumber)
            .IsUnique()
            .HasDatabaseName(DBKeys.Hardware.InventoryNumberUniqueIndex);

        builder.HasIndex(x => new { x.CabinetId, x.Type })
            .HasDatabaseName(DBKeys.Hardware.CabinetTypeIndex);

        builder.HasOne(x => x.Cabinet)
            .WithMany(x => x.Hardware)
            .HasForeignKey(x => x.CabinetId)
            .HasConstraintName(DBKeys.Hardware.CabinetForeignKey)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
