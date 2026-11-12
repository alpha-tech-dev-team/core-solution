using Atlas.Infrastructure.Database.Constants;
using Atlas.Domain.Entities;
using Atlas.Domain.Enums;
using Atlas.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Database.Context;

public sealed class AtlasDbContext : DbContext
{
    public DbSet<Room> Rooms { get; set; } = default!;
    public DbSet<Hardware> Hardware { get; set; } = default!;
    public DbSet<EquipmentType> EquipmentTypes { get; set; } = default!;
    public DbSet<Software> Software { get; set; } = default!;
    public DbSet<Employee> Employees { get; set; } = default!;
    public DbSet<IncidentCategory> IncidentCategories { get; set; } = default!;
    public DbSet<Incident> Incidents { get; set; } = default!;
    public DbSet<IncidentStatusHistory> IncidentStatusHistory { get; set; } = default!;
    public DbSet<IncidentFeedback> IncidentFeedback { get; set; } = default!;
    public DbSet<KnowledgeArticle> KnowledgeArticles { get; set; } = default!;
    public DbSet<KnowledgeArticleVector> KnowledgeArticleVectors { get; set; } = default!;

    public AtlasDbContext(DbContextOptions<AtlasDbContext> options)
        : base(options)
    {
    }

    public override int SaveChanges()
    {
        ApplyUpdatedAt();

        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyUpdatedAt();

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension(DatabaseConstants.Defaults.VectorExtension);
        modelBuilder.HasPostgresEnum<EmployeeRole>(name: DatabaseConstants.EnumNames.EmployeeRole);
        modelBuilder.HasPostgresEnum<IncidentStatus>(name: DatabaseConstants.EnumNames.IncidentStatus);
        modelBuilder.HasPostgresEnum<IncidentPriority>(name: DatabaseConstants.EnumNames.IncidentPriority);
        modelBuilder.HasPostgresEnum<KnowledgeArticleStatus>(name: DatabaseConstants.EnumNames.KnowledgeArticleStatus);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AtlasDbContext).Assembly);
    }

    private void ApplyUpdatedAt()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IHasUpdatedAt>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}


