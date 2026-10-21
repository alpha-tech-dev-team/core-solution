namespace Pandora.Infrastructure.Secrets;

public sealed class SecretsOptions
{
    public const string SectionName = "Secrets";

    public List<SecretItemOptions> Items { get; init; } = [];
}
