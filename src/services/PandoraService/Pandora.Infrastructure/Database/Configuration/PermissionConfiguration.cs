using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pandora.Core.Entities;

namespace Pandora.Infrastructure.Database.Configuration;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    private const int VaultItemIdMaxLength = 36;

    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable(DatabaseConstants.Permissions.Table);
        builder.HasKey(e => new { e.UserId, e.VaultItemId });
        builder.Property(e => e.UserId).HasColumnName(DatabaseConstants.Permissions.UserId);
        builder.Property(e => e.VaultItemId).HasColumnName(DatabaseConstants.Permissions.VaultItemId).HasMaxLength(VaultItemIdMaxLength);
        builder.Property(e => e.IsActive).HasColumnName(DatabaseConstants.Permissions.IsActive);
        builder.HasOne<SecretMetadata>().WithMany().HasForeignKey(e => e.VaultItemId).OnDelete(DeleteBehavior.Cascade);
    }
}
