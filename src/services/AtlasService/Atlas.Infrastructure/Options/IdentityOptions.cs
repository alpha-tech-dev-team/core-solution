namespace Atlas.Infrastructure.Options;

public sealed class IdentityOptions
{
    public const string SectionName = "Identity";
    public string BaseUrl {get; set;} = string.Empty;
}