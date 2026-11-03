namespace Pandora.Domain.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public string RedisConnection { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
}
