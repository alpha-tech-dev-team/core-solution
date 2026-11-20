namespace Pandora.Infrastructure.Secrets.Vaultwarden;

internal sealed record VaultwardenList(string? Object, IReadOnlyList<VaultwardenListItem>? Data);
