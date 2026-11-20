using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pandora.Domain.Entities;

namespace Pandora.Infrastructure.Database.Configuration;

public sealed class CredentialAccessMessageConfiguration : IEntityTypeConfiguration<CredentialAccessMessage>
{
    public void Configure(EntityTypeBuilder<CredentialAccessMessage> builder)
    {
        builder.ToTable(DatabaseConstants.CredentialAccessMessages.Table);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName(DatabaseConstants.CredentialAccessMessages.Id);
        builder.Property(e => e.ChatId).HasColumnName(DatabaseConstants.CredentialAccessMessages.ChatId);
        builder.Property(e => e.MessageId).HasColumnName(DatabaseConstants.CredentialAccessMessages.MessageId);
        builder.Property(e => e.ItemId)
            .HasColumnName(DatabaseConstants.CredentialAccessMessages.ItemId)
            .HasMaxLength(DatabaseConstants.CredentialAccessMessages.ItemIdMaxLength);
        builder.Property(e => e.ItemName)
            .HasColumnName(DatabaseConstants.CredentialAccessMessages.ItemName)
            .HasMaxLength(DatabaseConstants.CredentialAccessMessages.ItemNameMaxLength);
        builder.Property(e => e.RequestedAt).HasColumnName(DatabaseConstants.CredentialAccessMessages.RequestedAt);
        builder.Property(e => e.ExpiresAt).HasColumnName(DatabaseConstants.CredentialAccessMessages.ExpiresAt);
        builder.Property(e => e.NextAttemptAt).HasColumnName(DatabaseConstants.CredentialAccessMessages.NextAttemptAt);
        builder.Property(e => e.DeletedAt).HasColumnName(DatabaseConstants.CredentialAccessMessages.DeletedAt);
        builder.Property(e => e.NotifiedAt).HasColumnName(DatabaseConstants.CredentialAccessMessages.NotifiedAt);
        builder.Property(e => e.Attempts).HasColumnName(DatabaseConstants.CredentialAccessMessages.Attempts);
        builder.Property(e => e.LastError)
            .HasColumnName(DatabaseConstants.CredentialAccessMessages.LastError)
            .HasMaxLength(DatabaseConstants.CredentialAccessMessages.LastErrorMaxLength);
        builder.HasIndex(e => e.NextAttemptAt);
    }
}
