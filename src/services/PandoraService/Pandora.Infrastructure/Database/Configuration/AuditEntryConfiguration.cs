using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pandora.Core.Entities;

namespace Pandora.Infrastructure.Database.Configuration;

public sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    private const int ActionMaxLength = 50;
    private const int TargetItemIdMaxLength = 36;
    private const int RequestSourceMaxLength = 20;

    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable(DatabaseConstants.Audit.Table);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName(DatabaseConstants.Audit.Id);
        builder.Property(e => e.Timestamp).HasColumnName(DatabaseConstants.Audit.Timestamp);
        builder.Property(e => e.UserId).HasColumnName(DatabaseConstants.Audit.UserId);
        builder.Property(e => e.Action).HasColumnName(DatabaseConstants.Audit.Action).HasMaxLength(ActionMaxLength);
        builder.Property(e => e.TargetItemId).HasColumnName(DatabaseConstants.Audit.TargetItemId).HasMaxLength(TargetItemIdMaxLength);
        builder.Property(e => e.RequestSource).HasColumnName(DatabaseConstants.Audit.RequestSource).HasMaxLength(RequestSourceMaxLength);
        builder.HasIndex(e => e.UserId);
    }
}
