using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pandora.Domain.Entities;

namespace Pandora.Infrastructure.Database.Configuration;

public sealed class AuditOutboxEntryConfiguration : IEntityTypeConfiguration<AuditOutboxEntry>
{
    public void Configure(EntityTypeBuilder<AuditOutboxEntry> builder)
    {
        builder.ToTable(DatabaseConstants.AuditOutbox.Table);
        builder.HasKey(entry => entry.Id);
        builder.Property(entry => entry.Id).HasColumnName(DatabaseConstants.AuditOutbox.Id);
        builder.Property(entry => entry.Payload).HasColumnName(DatabaseConstants.AuditOutbox.Payload);
        builder.Property(entry => entry.CreatedAt).HasColumnName(DatabaseConstants.AuditOutbox.CreatedAt);
        builder.HasIndex(entry => entry.CreatedAt);
    }
}
