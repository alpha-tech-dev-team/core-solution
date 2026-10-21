namespace Pandora.Infrastructure.Access;

public sealed class AccessUserOptions
{
    public long ChatId { get; init; }

    public List<string> Services { get; init; } = [];
}
