using Atlas.Infrastructure.Database.Constants;
using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class IncidentCategoryConfiguration : IEntityTypeConfiguration<IncidentCategory>
{
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

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
