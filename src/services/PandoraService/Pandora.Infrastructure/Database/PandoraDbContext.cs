using Microsoft.EntityFrameworkCore;
using Pandora.Core.Entities;
using Pandora.Infrastructure.Database.Configuration;

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

        modelBuilder.ApplyConfiguration(new SecretMetadataConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new AuditEntryConfiguration());
    }
}
