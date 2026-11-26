using System.Globalization;

namespace Pandora.Infrastructure.Database;

public static class PostgresLikePattern
{
    public const string EscapeCharacter = "\\";

    private const string ContainsTemplate = "%{0}%";
    private const string EscapedEscapeCharacter = "\\\\";
    private const string PercentCharacter = "%";
    private const string EscapedPercentCharacter = "\\%";
    private const string UnderscoreCharacter = "_";
    private const string EscapedUnderscoreCharacter = "\\_";

    public static string Contains(string value)
    {
        return string.Format(CultureInfo.InvariantCulture, ContainsTemplate, Escape(value));
    }

    private static string Escape(string value)
    {
        return value
            .Replace(EscapeCharacter, EscapedEscapeCharacter, StringComparison.Ordinal)
            .Replace(PercentCharacter, EscapedPercentCharacter, StringComparison.Ordinal)
            .Replace(UnderscoreCharacter, EscapedUnderscoreCharacter, StringComparison.Ordinal);
    }
}
