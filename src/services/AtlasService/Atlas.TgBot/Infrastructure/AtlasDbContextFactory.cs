using Atlas.Domain.Options;
using Atlas.Infrastructure.Database.Context;
using Atlas.Infrastructure.Database.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Pgvector.EntityFrameworkCore;

namespace Atlas.TgBot.Infrastructure;

public sealed class AtlasDbContextFactory : IDesignTimeDbContextFactory<AtlasDbContext>
{
    public AtlasDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(DesignTimeConfigurationConstants.AppSettingsFile, optional: true)
            .AddJsonFile(BuildEnvironmentAppSettingsFile(), optional: true)
            .AddEnvironmentVariables()
            .Build();

        var databaseOptions = configuration
            .GetSection(DatabaseOptions.SectionName)
            .Get<DatabaseOptions>();

        ArgumentNullException.ThrowIfNull(
            databaseOptions,
            DatabaseOptions.SectionName);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            databaseOptions.ConnectionString,
            BuildConnectionStringParameterName());

        var options = new DbContextOptionsBuilder<AtlasDbContext>()
            .UseNpgsql(databaseOptions.ConnectionString, npgsql =>
            {
                npgsql.UseVector();
                npgsql.MapAtlasEnums();
            })
            .Options;

        return new AtlasDbContext(options);
    }

    private static string BuildEnvironmentAppSettingsFile()
    {
        var environmentName = Environment.GetEnvironmentVariable(
            DesignTimeConfigurationConstants.EnvironmentVariableName);

        return string.Format(
            DesignTimeConfigurationConstants.EnvironmentAppSettingsFilePattern,
            environmentName);
    }

    private static string BuildConnectionStringParameterName()
    {
        return $"{DatabaseOptions.SectionName}:{nameof(DatabaseOptions.ConnectionString)}";
    }
}
