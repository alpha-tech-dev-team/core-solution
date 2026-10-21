using Identity.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Database.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Surname)
            .HasColumnName("surname")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Patronymic)
            .HasColumnName("patronymic")
            .HasMaxLength(50);

        builder.Property(x => x.Position)
            .HasColumnName("position")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Phone)
            .HasColumnName("phone_number")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.TelegramUserId)
            .HasColumnName("telegram_user_id");

        builder.Property(x => x.AdminUsername)
            .HasColumnName("admin_login")
            .HasMaxLength(50);

        builder.Property(x => x.AdminPasswordHash)
            .HasColumnName("admin_password_hash");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedById)
            .HasColumnName("updated_by_id")
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
