using Identity.Core.Entities;
using Identity.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Database.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(DatabaseConstants.Tables.Users);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName(DatabaseConstants.Columns.Id);

        builder.Property(x => x.Name)
            .HasColumnName(DatabaseConstants.Columns.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Surname)
            .HasColumnName(DatabaseConstants.Columns.Surname)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Patronymic)
            .HasColumnName(DatabaseConstants.Columns.Patronymic)
            .HasMaxLength(50);

        builder.Property(x => x.Position)
            .HasColumnName(DatabaseConstants.Columns.Position)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Phone)
            .HasColumnName(DatabaseConstants.Columns.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.TelegramUserId)
            .HasColumnName(DatabaseConstants.Columns.TelegramUserId);

        builder.Property(x => x.Status)
            .HasColumnName(DatabaseConstants.Columns.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(UserStatus.Pending)
            .IsRequired();

        builder.Property(x => x.AdminUsername)
            .HasColumnName(DatabaseConstants.Columns.AdminLogin)
            .HasMaxLength(50);

        builder.Property(x => x.AdminPasswordHash)
            .HasColumnName(DatabaseConstants.Columns.AdminPasswordHash);

        builder.Property(x => x.CreatedAt)
            .HasColumnName(DatabaseConstants.Columns.CreatedAt)
            .HasColumnType(DatabaseConstants.TimestampWithTimeZone)
            .HasDefaultValueSql(DatabaseConstants.CurrentTimestampSql);

        builder.Property(x => x.UpdatedAt)
            .HasColumnName(DatabaseConstants.Columns.UpdatedAt)
            .HasColumnType(DatabaseConstants.TimestampWithTimeZone)
            .HasDefaultValueSql(DatabaseConstants.CurrentTimestampSql);

        builder.Property(x => x.UpdatedById)
            .HasColumnName(DatabaseConstants.Columns.UpdatedById)
            .IsRequired();

        builder.HasIndex(x => x.Phone)
            .IsUnique();

        builder.HasIndex(x => x.TelegramUserId)
            .IsUnique();

        builder.HasIndex(x => x.AdminUsername)
            .IsUnique();

        builder.HasOne(x => x.UpdatedBy)
            .WithMany()
            .HasForeignKey(x => x.UpdatedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
