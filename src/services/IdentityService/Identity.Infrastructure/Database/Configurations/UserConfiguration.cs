using Identity.Core.Entities;
using Identity.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Database.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(DatabaseConstants.Users.Table);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName(DatabaseConstants.Users.Id);

        builder.Property(x => x.Name)
            .HasColumnName(DatabaseConstants.Users.Name)
            .HasMaxLength(DatabaseConstants.Users.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Surname)
            .HasColumnName(DatabaseConstants.Users.Surname)
            .HasMaxLength(DatabaseConstants.Users.SurnameMaxLength)
            .IsRequired();

        builder.Property(x => x.Patronymic)
            .HasColumnName(DatabaseConstants.Users.Patronymic)
            .HasMaxLength(DatabaseConstants.Users.PatronymicMaxLength);

        builder.Property(x => x.Position)
            .HasColumnName(DatabaseConstants.Users.Position)
            .HasMaxLength(DatabaseConstants.Users.PositionMaxLength)
            .IsRequired();

        builder.Property(x => x.Phone)
            .HasColumnName(DatabaseConstants.Users.PhoneNumber)
            .HasMaxLength(DatabaseConstants.Users.PhoneMaxLength)
            .IsRequired();

        builder.Property(x => x.TelegramUserId)
            .HasColumnName(DatabaseConstants.Users.TelegramUserId);

        builder.Property(x => x.Status)
            .HasColumnName(DatabaseConstants.Users.Status)
            .HasConversion<string>()
            .HasMaxLength(DatabaseConstants.Users.StatusMaxLength)
            .HasDefaultValue(UserStatus.Pending)
            .IsRequired();

        builder.Property(x => x.AdminUsername)
            .HasColumnName(DatabaseConstants.Users.AdminLogin)
            .HasMaxLength(DatabaseConstants.Users.AdminLoginMaxLength);

        builder.Property(x => x.AdminPasswordHash)
            .HasColumnName(DatabaseConstants.Users.AdminPasswordHash);

        builder.Property(x => x.CreatedAt)
            .HasColumnName(DatabaseConstants.Users.CreatedAt)
            .HasColumnType(DatabaseConstants.TimestampWithTimeZone)
            .HasDefaultValueSql(DatabaseConstants.CurrentTimestampSql);

        builder.Property(x => x.UpdatedAt)
            .HasColumnName(DatabaseConstants.Users.UpdatedAt)
            .HasColumnType(DatabaseConstants.TimestampWithTimeZone)
            .HasDefaultValueSql(DatabaseConstants.CurrentTimestampSql);

        builder.Property(x => x.UpdatedById)
            .HasColumnName(DatabaseConstants.Users.UpdatedById)
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
