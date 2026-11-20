using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pandora.Domain.Entities;

namespace Pandora.Infrastructure.Database.Configuration;

public sealed class AuditOutboxEntryConfiguration : IEntityTypeConfiguration<AuditOutboxEntry>
{
    public void Configure(EntityTypeBuilder<AuditOutboxEntry> builder)
    {
        builder.ToTable(DatabaseConstants.AuditOutbox.Table);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName(DatabaseConstants.AuditOutbox.Id);
        builder.Property(e => e.Payload).HasColumnName(DatabaseConstants.AuditOutbox.Payload);
        builder.Property(e => e.CreatedAt).HasColumnName(DatabaseConstants.AuditOutbox.CreatedAt);
        builder.HasIndex(e => e.CreatedAt);
    }
}
