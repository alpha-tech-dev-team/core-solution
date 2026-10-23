namespace Atlas.Infrastructure.Options;

public sealed class AccessOptions
{
    public const string SectionName = "Access";

    public List<long> AllowedTgUserIds { get; init; } = [];
}