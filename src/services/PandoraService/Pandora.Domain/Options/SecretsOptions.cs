namespace Pandora.Domain.Options;

public sealed class SecretsOptions
{
    public const string SectionName = "Pandora:Secrets";

    public bool UseMock { get; init; }
    public List<SecretItemOptions> Items { get; init; } = new();
}
