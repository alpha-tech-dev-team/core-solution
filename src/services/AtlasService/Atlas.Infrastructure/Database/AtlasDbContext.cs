using Atlas.Domain.Constants.AtlasDBConfiguration;
using Atlas.Domain.Entities;
using Atlas.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Database;

public sealed class AtlasDbContext : DbContext
{
    public DbSet<Cabinet> Cabinets
    {
        get { return Set<Cabinet>(); }
    }

    public DbSet<Hardware> Hardware
    {
        get { return Set<Hardware>(); }
    }

    public DbSet<Software> Software
    {
        get { return Set<Software>(); }
    }

    public DbSet<Employee> Employees
    {
        get { return Set<Employee>(); }
    }

    public DbSet<IncidentCategory> IncidentCategories
    {
        get { return Set<IncidentCategory>(); }
    }

    public DbSet<Incident> Incidents
    {
        get { return Set<Incident>(); }
    }

    public DbSet<IncidentStatusHistory> IncidentStatusHistory
    {
        get { return Set<IncidentStatusHistory>(); }
    }

    public DbSet<IncidentFeedback> IncidentFeedback
    {
        get { return Set<IncidentFeedback>(); }
    }

    public DbSet<KnowledgeArticle> KnowledgeArticles
    {
        get { return Set<KnowledgeArticle>(); }
    }

    public DbSet<KnowledgeArticleVector> KnowledgeArticleVectors
    {
        get { return Set<KnowledgeArticleVector>(); }
    }

    public AtlasDbContext(DbContextOptions<AtlasDbContext> options) : base(options)
    {
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await ValidateIncidentAssetsAsync(cancellationToken);
        ApplyUpdatedAt();

        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidateIncidentAssetsAsync(CancellationToken cancellationToken)
    {
        var incidents = ChangeTracker.Entries<Incident>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
            .Select(entry => entry.Entity)
            .ToList();

        if (incidents.Count == 0)
        {
            return;
        }

        var categoryIds = incidents.Select(x => x.CategoryId).Distinct().ToList();
        var categories = await IncidentCategories
            .Where(category => categoryIds.Contains(category.Id))
            .ToDictionaryAsync(category => category.Id, category => category.Name, cancellationToken);

        foreach (var incident in incidents)
        {
            if (!categories.TryGetValue(incident.CategoryId, out var categoryName))
            {
                throw new InvalidOperationException($"Incident category '{incident.CategoryId}' was not found.");
            }

            var (requiresHardware, requiresSoftware) = categoryName switch
            {
                DBKeys.IncidentCategory.HardwareName => (true, false),
                DBKeys.IncidentCategory.SoftwareName => (false, true),
                DBKeys.IncidentCategory.OtherName => (false, false),
                _ => throw new InvalidOperationException($"Unknown incident category '{categoryName}'.")
            };

            if (requiresHardware && incident.HardwareId is null)
            {
                throw new InvalidOperationException("Hardware is required for the 'hardware' incident category.");
            }

            if (!requiresHardware && incident.HardwareId is not null)
            {
                throw new InvalidOperationException("Hardware is not allowed for this incident category.");
            }

            if (requiresSoftware && incident.SoftwareId is null)
            {
                throw new InvalidOperationException("Software is required for the 'software' incident category.");
            }

            if (!requiresSoftware && incident.SoftwareId is not null)
            {
                throw new InvalidOperationException("Software is not allowed for this incident category.");
            }
        }
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(DBKeys.Shared.Schema);
        modelBuilder.HasPostgresExtension(DBKeys.Shared.VectorExtension);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AtlasDbContext).Assembly);
    }
}
