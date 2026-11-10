using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pandora.Domain.Entities;

namespace Pandora.Infrastructure.Database.Configuration;

public sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable(DatabaseConstants.Audit.Table);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName(DatabaseConstants.Audit.Id);
        builder.Property(e => e.Timestamp).HasColumnName(DatabaseConstants.Audit.Timestamp);
        builder.Property(e => e.TelegramUserId).HasColumnName(DatabaseConstants.Audit.TelegramUserId);
        builder.Property(e => e.Action).HasColumnName(DatabaseConstants.Audit.Action).HasMaxLength(DatabaseConstants.Audit.ActionMaxLength);
        builder.Property(e => e.TargetItemId).HasColumnName(DatabaseConstants.Audit.TargetItemId).HasMaxLength(DatabaseConstants.Audit.TargetItemIdMaxLength);
        builder.Property(e => e.RequestSource).HasColumnName(DatabaseConstants.Audit.RequestSource).HasMaxLength(DatabaseConstants.Audit.RequestSourceMaxLength);
        builder.HasIndex(e => e.TelegramUserId);
    }
}
