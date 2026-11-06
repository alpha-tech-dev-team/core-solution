using Atlas.Domain.Constants.AtlasDBConfiguration;
using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class CabinetConfiguration : IEntityTypeConfiguration<Cabinet>
{
    public void Configure(EntityTypeBuilder<Cabinet> builder)
    {
        builder.ToTable(DBKeys.Cabinet.Table, DBKeys.Shared.Schema);

        builder.HasKey(x => x.Id).HasName(DBKeys.Cabinet.PrimaryKey);
        builder.Property(x => x.Id)
            .HasColumnName(DBKeys.Shared.Id)
            .HasDefaultValueSql(DBKeys.Shared.GenRandomUuidSql);

        builder.Property(x => x.Number)
            .HasColumnName(DBKeys.Cabinet.Number)
            .HasMaxLength(DBKeys.Shared.MaxLength50)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName(DBKeys.Cabinet.Description);

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

        builder.HasIndex(x => x.Number)
            .IsUnique()
            .HasDatabaseName(DBKeys.Cabinet.NumberUniqueIndex);
    }
}
