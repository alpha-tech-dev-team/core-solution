namespace Pandora.Core.Options;

public sealed class SessionOptions
{
    public const string SectionName = "SessionSettings";

    public int ExpiryMinutes { get; set; }
}
