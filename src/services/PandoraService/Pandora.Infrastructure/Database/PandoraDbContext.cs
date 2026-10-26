using Microsoft.EntityFrameworkCore;
using Pandora.Core.Entities;

namespace Pandora.Infrastructure.Database;

public sealed class PandoraDbContext : DbContext
{
    public PandoraDbContext(DbContextOptions<PandoraDbContext> options) : base(options) { }

    public DbSet<SecretMetadata> SecretMetadata { get; set; } = null!;
    public DbSet<Permission> Permission { get; set; } = null!;
    public DbSet<AuditEntry> AuditEntry { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SecretMetadata>(entity =>
        {
            entity.ToTable(DatabaseConstants.SecretMetadata.Table);
            entity.HasKey(e => e.VaultItemId);
            entity.Property(e => e.VaultItemId).HasColumnName(DatabaseConstants.SecretMetadata.VaultItemId).HasMaxLength(36);
            entity.Property(e => e.ResourceName).HasColumnName(DatabaseConstants.SecretMetadata.ResourceName).HasMaxLength(100);
            entity.Property(e => e.Username).HasColumnName(DatabaseConstants.SecretMetadata.Username).HasMaxLength(100);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable(DatabaseConstants.Permissions.Table);
            entity.HasKey(e => new { e.UserId, e.VaultItemId });
            entity.Property(e => e.UserId).HasColumnName(DatabaseConstants.Permissions.UserId);
            entity.Property(e => e.VaultItemId).HasColumnName(DatabaseConstants.Permissions.VaultItemId).HasMaxLength(36);
            entity.Property(e => e.IsActive).HasColumnName(DatabaseConstants.Permissions.IsActive);
            entity.HasOne<SecretMetadata>().WithMany().HasForeignKey(e => e.VaultItemId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditEntry>(entity =>
        {
            entity.ToTable(DatabaseConstants.Audit.Table);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName(DatabaseConstants.Audit.Id);
            entity.Property(e => e.Timestamp).HasColumnName(DatabaseConstants.Audit.Timestamp);
            entity.Property(e => e.UserId).HasColumnName(DatabaseConstants.Audit.UserId);
            entity.Property(e => e.Action).HasColumnName(DatabaseConstants.Audit.Action).HasMaxLength(50);
            entity.Property(e => e.TargetItemId).HasColumnName(DatabaseConstants.Audit.TargetItemId).HasMaxLength(36);
            entity.Property(e => e.RequestSource).HasColumnName(DatabaseConstants.Audit.RequestSource).HasMaxLength(20);
            entity.HasIndex(e => e.UserId);
        });
    }
}
