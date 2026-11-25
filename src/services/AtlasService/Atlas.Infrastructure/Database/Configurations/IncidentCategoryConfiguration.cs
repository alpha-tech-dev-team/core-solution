using Atlas.Infrastructure.Database.Constants;
using Atlas.Domain.Entities;
using Atlas.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class IncidentCategoryConfiguration : IEntityTypeConfiguration<IncidentCategory>
{
    private const int UnsetTypeValue = -1;
    private const IncidentCategoryType UnsetTypeSentinel = (IncidentCategoryType)UnsetTypeValue;

    public void Configure(EntityTypeBuilder<IncidentCategory> builder)
    {
        builder.ToTable(DatabaseConstants.IncidentCategory.Table);

        builder.Property(x => x.Id)
            .HasColumnName(DatabaseConstants.IncidentCategory.Id)
            .HasDefaultValueSql(DatabaseConstants.Defaults.GenRandomUuidSql);

        builder.Property(x => x.Name)
            .HasColumnName(DatabaseConstants.IncidentCategory.Name)
            .HasMaxLength(DatabaseConstants.IncidentCategory.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName(DatabaseConstants.IncidentCategory.Description);

        builder.Property(x => x.Type)
            .HasColumnName(DatabaseConstants.IncidentCategory.Type)
            .HasDefaultValue(IncidentCategoryType.Other)
            .HasSentinel(UnsetTypeSentinel)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
