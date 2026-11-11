namespace Pandora.Infrastructure.Secrets.Vaultwarden;

internal sealed record VaultwardenItem(
    string? Id,
    string? Name,
    string? OrganizationId,
    VaultwardenLogin? Login,
    List<string>? CollectionIds,
    bool Favorite,
    int Reprompt,
    string? Notes,
    DateTime? DeletedDate)
{
    public static VaultwardenItem Empty { get; } = new(null, null, null, null, null, false, 0, null, null);
}
