using Atlas.Domain.Constants.AtlasDBConfiguration;
using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class OfficeConfiguration : IEntityTypeConfiguration<Office>
{
    public void Configure(EntityTypeBuilder<Office> builder)
    {
        builder.ToTable(DBKeys.Office.Table, DBKeys.Shared.Schema);

        builder.HasKey(x => x.Id).HasName(DBKeys.Office.PrimaryKey);
        builder.Property(x => x.Id)
            .HasColumnName(DBKeys.Shared.Id)
            .HasDefaultValueSql(DBKeys.Shared.GenRandomUuidSql);

        builder.Property(x => x.RoomNumber)
            .HasColumnName(DBKeys.Office.RoomNumber)
            .HasMaxLength(DBKeys.Shared.MaxLength50)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName(DBKeys.Office.Description);

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

        builder.HasIndex(x => x.RoomNumber)
            .IsUnique()
            .HasDatabaseName(DBKeys.Office.RoomNumberUniqueIndex);
    }
}
