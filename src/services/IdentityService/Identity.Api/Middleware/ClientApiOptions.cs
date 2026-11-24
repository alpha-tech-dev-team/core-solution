namespace Identity.Api.Middleware;

public sealed class ClientApiOptions
{
    public const string SectionName = "ClientApi";

    public string AtlasApiKey { get; set; } = string.Empty;
}
