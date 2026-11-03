namespace Pandora.Infrastructure.Secrets;

public sealed class SecretsOptions
{
    public const string SectionName = "Pandora:Secrets";

    public bool UseMock { get; init; }
    public List<SecretItemOptions> Items { get; init; } = [];
}
