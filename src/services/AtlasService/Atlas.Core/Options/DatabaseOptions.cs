namespace Atlas.Core.Options;

public class DatabaseOptions
{
    public const string SectionName = "Database";
    public string RedisConnection { get; set; } = string.Empty;
}
