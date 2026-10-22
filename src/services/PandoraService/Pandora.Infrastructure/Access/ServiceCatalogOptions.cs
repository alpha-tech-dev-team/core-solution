namespace Pandora.Infrastructure.Access;

public sealed class ServiceCatalogOptions
{
    public const string SectionName = "Pandora:Services";

    public List<string> Items { get; init; } = [];
}
