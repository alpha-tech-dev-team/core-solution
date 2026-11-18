using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Pandora.Infrastructure.Database;

public sealed class PandoraDbContextFactory : IDesignTimeDbContextFactory<PandoraDbContext>
{
    private const string ConnectionStringEnvironmentVariable = "Database__ConnectionString";

    public PandoraDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Set the {ConnectionStringEnvironmentVariable} environment variable to run EF design-time commands.");
        }

        var options = new DbContextOptionsBuilder<PandoraDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new PandoraDbContext(options);
    }
}
