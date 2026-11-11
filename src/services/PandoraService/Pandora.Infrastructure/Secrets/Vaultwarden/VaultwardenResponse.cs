namespace Pandora.Infrastructure.Secrets.Vaultwarden;

internal sealed record VaultwardenResponse<T>(T? Data);
