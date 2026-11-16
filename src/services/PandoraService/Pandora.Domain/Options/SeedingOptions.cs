namespace Pandora.Domain.Options;

public sealed class SeedingOptions
{
    public const string SectionName = "Seeding";

    public bool DevelopmentData { get; set; }
}
