namespace Atlas.Core.Options;

public class SessionOptions
{
    public const string SectionName = "SessionSettings";
    public int ExpiryMinutes { get; set; }
}
