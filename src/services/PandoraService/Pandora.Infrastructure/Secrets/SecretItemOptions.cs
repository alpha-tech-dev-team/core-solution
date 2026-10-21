namespace Pandora.Infrastructure.Secrets;

public sealed class SecretItemOptions
{
    public string Name { get; init; } = string.Empty;
    public string Login { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
