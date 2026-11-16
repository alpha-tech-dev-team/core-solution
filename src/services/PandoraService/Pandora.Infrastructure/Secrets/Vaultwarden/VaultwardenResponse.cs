namespace Pandora.Infrastructure.Secrets.Vaultwarden;

internal sealed record VaultwardenResponse<T>(bool Success, T? Data);
