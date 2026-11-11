namespace Atlas.Domain.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public string ConnectionString { get; set; } = string.Empty;

    public bool SeedOnStartup { get; set; }
}
