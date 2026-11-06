using Atlas.Domain.Constants.AtlasDBConfiguration;
using Atlas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Database;

public sealed class AtlasDbContext : DbContext
{
    public DbSet<Office> Offices
    {
        get { return Set<Office>(); }
    }

    public DbSet<Hardware> Hardware
    {
        get { return Set<Hardware>(); }
    }

    public DbSet<Software> Software
    {
        get { return Set<Software>(); }
    }

    public DbSet<EmployeeAccess> EmployeeAccess
    {
        get { return Set<EmployeeAccess>(); }
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(DBKeys.Shared.Schema);
        modelBuilder.HasPostgresExtension(DBKeys.Shared.VectorExtension);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AtlasDbContext).Assembly);
    }
}
