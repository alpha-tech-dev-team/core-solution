using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Pandora.Infrastructure.Database;

public sealed class PandoraDbContextFactory : IDesignTimeDbContextFactory<PandoraDbContext>
{
    public PandoraDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PandoraDbContext>()
            .UseNpgsql("Host=localhost;Database=pandora;Username=postgres")
            .Options;

        return new PandoraDbContext(options);
    }
}
