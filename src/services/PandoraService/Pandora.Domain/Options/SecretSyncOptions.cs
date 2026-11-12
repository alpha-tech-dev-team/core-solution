namespace Pandora.Domain.Options;

public sealed class SecretSyncOptions
{
    public const string SectionName = "Pandora:Secrets:Sync";

    public bool Enabled { get; set; } = true;
    public int IntervalSeconds { get; set; } = 60;
}
