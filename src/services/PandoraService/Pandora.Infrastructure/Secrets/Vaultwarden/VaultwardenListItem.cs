namespace Pandora.Infrastructure.Secrets.Vaultwarden;

internal sealed record VaultwardenListItem(string Id, string Name, VaultwardenLogin? Login);
