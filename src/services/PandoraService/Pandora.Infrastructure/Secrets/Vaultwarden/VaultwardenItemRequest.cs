namespace Pandora.Infrastructure.Secrets.Vaultwarden;

internal sealed class VaultwardenItemRequest
{
    public string? Id { get; init; }
    public string? OrganizationId { get; init; }
    public int Type { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public bool Favorite { get; init; }
    public int Reprompt { get; init; }
    public VaultwardenLoginRequest? Login { get; init; }
    public List<string> CollectionIds { get; init; } = new();
}
