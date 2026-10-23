namespace Pandora.Infrastructure.Identity;

public sealed class IdentityOptions
{
    public const string SectionName = "Pandora:Identity";

    public string BaseUrl { get; set; } = string.Empty;
}
