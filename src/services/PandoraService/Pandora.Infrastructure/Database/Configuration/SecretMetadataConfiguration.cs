using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pandora.Domain.Entities;
using Pandora.Domain.Enums;

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
        builder.Property(e => e.SyncOperation)
            .HasColumnName(DatabaseConstants.SecretMetadata.SyncOperation)
            .HasMaxLength(DatabaseConstants.SecretMetadata.SyncOperationMaxLength)
            .HasConversion<string>()
            .HasDefaultValue(SecretSyncOperation.None);
        builder.Property(e => e.AttemptCount)
            .HasColumnName(DatabaseConstants.SecretMetadata.AttemptCount)
            .HasDefaultValue(SecretMetadata.DefaultAttemptCount);
        builder.Property(e => e.NextAttemptAt).HasColumnName(DatabaseConstants.SecretMetadata.NextAttemptAt);
        builder.Property(e => e.LastError)
            .HasColumnName(DatabaseConstants.SecretMetadata.LastError)
            .HasMaxLength(DatabaseConstants.SecretMetadata.LastErrorMaxLength);
    }
}
