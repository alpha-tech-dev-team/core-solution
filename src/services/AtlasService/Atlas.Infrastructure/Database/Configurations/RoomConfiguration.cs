using Atlas.Infrastructure.Database.Constants;
using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Database.Configurations;

public sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable(DatabaseConstants.Room.Table);

        builder.Property(x => x.Id)
            .HasColumnName(DatabaseConstants.Room.Id)
            .HasDefaultValueSql(DatabaseConstants.Defaults.GenRandomUuidSql);

        builder.Property(x => x.Name)
            .HasColumnName(DatabaseConstants.Room.Name)
            .HasMaxLength(DatabaseConstants.Room.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName(DatabaseConstants.Room.Description);

        builder.Property(x => x.CreatedAt)
            .HasColumnName(DatabaseConstants.Room.CreatedAt)
            .HasDefaultValueSql(DatabaseConstants.Defaults.NowSql);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
