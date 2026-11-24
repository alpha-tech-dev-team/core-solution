using Atlas.Infrastructure.Database.Constants;
using Atlas.Domain.Entities;
using Atlas.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class IncidentCategoryConfiguration : IEntityTypeConfiguration<IncidentCategory>
{
    /// <summary>
    /// Сентинел EF: если свойство равно этому значению, столбец не передаётся и БД подставляет
    /// HasDefaultValue(Other). -1 не совпадает ни с одним значением incident_category_type,
    /// поэтому явно заданные Hardware/Software/Other всегда пишутся в БД.
    /// </summary>
    private const IncidentCategoryType UnsetTypeSentinel = (IncidentCategoryType)(-1);

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
