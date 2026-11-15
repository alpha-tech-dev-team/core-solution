using Atlas.Domain.Entities;
using Atlas.Infrastructure.Database.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class EquipmentTypeConfiguration : IEntityTypeConfiguration<EquipmentType>
{
    public void Configure(EntityTypeBuilder<EquipmentType> builder)
    {
        builder.ToTable(DatabaseConstants.EquipmentType.Table);

        builder.Property(x => x.Id)
            .HasColumnName(DatabaseConstants.EquipmentType.Id)
            .HasDefaultValueSql(DatabaseConstants.Defaults.GenRandomUuidSql);

        builder.Property(x => x.Name)
            .HasColumnName(DatabaseConstants.EquipmentType.Name)
            .HasMaxLength(DatabaseConstants.EquipmentType.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName(DatabaseConstants.EquipmentType.Description);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
