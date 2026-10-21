namespace Pandora.Infrastructure.Access;

public sealed class AccessOptions
{
    public const string SectionName = "Access";

    public List<long> AllowedChatIds { get; init; } = [];
}
