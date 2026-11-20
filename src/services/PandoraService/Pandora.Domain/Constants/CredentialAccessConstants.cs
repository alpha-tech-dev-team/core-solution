namespace Pandora.Domain.Constants;

public static class CredentialAccessConstants
{
    public const int DefaultTtlMinutes = 3;
    public const int SettingsRowId = 1;

    public static readonly IReadOnlyList<int> AllowedTtlMinutes = [2, 3, 5];
}
