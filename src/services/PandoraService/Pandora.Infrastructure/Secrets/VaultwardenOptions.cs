namespace Pandora.Infrastructure.Secrets;

public sealed class VaultwardenOptions
{
    public const string SectionName = "Pandora:Vaultwarden";

    public string BaseUrl { get; set; } = string.Empty;
}
