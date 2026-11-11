namespace Pandora.Infrastructure.Secrets.Vaultwarden;

internal sealed class VaultwardenLoginRequest
{
    public string? Username { get; init; }
    public string? Password { get; init; }
    public List<VaultwardenUri> SecurityAnswers { get; init; } = new();
    public List<VaultwardenUri> Uris { get; init; } = new();
}
