using Atlas.Infrastructure.Database.Constants;
using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class SoftwareConfiguration : IEntityTypeConfiguration<Software>
{
    public void Configure(EntityTypeBuilder<Software> builder)
    {
        builder.ToTable(DatabaseConstants.Software.Table);

        builder.Property(x => x.Id)
            .HasColumnName(DatabaseConstants.Software.Id)
            .HasDefaultValueSql(DatabaseConstants.Defaults.GenRandomUuidSql);

        builder.Property(x => x.Name)
            .HasColumnName(DatabaseConstants.Software.Name)
            .HasMaxLength(DatabaseConstants.Software.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Version)
            .HasColumnName(DatabaseConstants.Software.Version)
            .HasMaxLength(DatabaseConstants.Software.VersionMaxLength);

        builder.HasIndex(x => new { x.Name, x.Version })
            .IsUnique()
            .AreNullsDistinct(false);
    }
}
