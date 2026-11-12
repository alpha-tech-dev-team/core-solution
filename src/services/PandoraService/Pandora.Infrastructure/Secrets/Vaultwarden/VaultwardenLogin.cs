namespace Pandora.Infrastructure.Secrets.Vaultwarden;

internal sealed record VaultwardenLogin(
    string? Username,
    string? Password,
    List<VaultwardenUri>? SecurityAnswers,
    List<VaultwardenUri>? Uris);
