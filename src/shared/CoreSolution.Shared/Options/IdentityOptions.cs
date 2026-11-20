namespace CoreSolution.Shared.Options;

public sealed class IdentityOptions
{
    public const string SectionName = "Identity";

    public string BaseUrl { get; set; } = string.Empty;
    public string? ServiceApiKey { get; set; }
}
