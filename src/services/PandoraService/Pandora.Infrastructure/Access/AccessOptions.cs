namespace Pandora.Infrastructure.Access;

public sealed class AccessOptions
{
    public const string SectionName = "Pandora:Access";
    public List<AccessUserOptions> Users { get; init; } = [];
}
