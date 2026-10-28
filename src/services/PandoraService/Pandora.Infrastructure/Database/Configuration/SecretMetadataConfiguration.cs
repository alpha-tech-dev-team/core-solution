using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pandora.Core.Entities;

namespace Pandora.Infrastructure.Database.Configuration;

public sealed class SecretMetadataConfiguration : IEntityTypeConfiguration<SecretMetadata>
{
    public void Configure(EntityTypeBuilder<SecretMetadata> builder)
    {
        builder.ToTable(DatabaseConstants.SecretMetadata.Table);
        builder.HasKey(e => e.VaultItemId);
        builder.Property(e => e.VaultItemId).HasColumnName(DatabaseConstants.SecretMetadata.VaultItemId).HasMaxLength(DatabaseConstants.SecretMetadata.VaultItemIdMaxLength);
        builder.Property(e => e.ResourceName).HasColumnName(DatabaseConstants.SecretMetadata.ResourceName).HasMaxLength(DatabaseConstants.SecretMetadata.ResourceNameMaxLength);
        builder.Property(e => e.Username).HasColumnName(DatabaseConstants.SecretMetadata.Username).HasMaxLength(DatabaseConstants.SecretMetadata.UsernameMaxLength);
    }
}
