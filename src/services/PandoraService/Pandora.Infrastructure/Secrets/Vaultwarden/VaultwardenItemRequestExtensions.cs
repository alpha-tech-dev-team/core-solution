namespace Pandora.Infrastructure.Secrets.Vaultwarden;

internal static class VaultwardenItemRequestExtensions
{
    private const int LoginItemType = 1;
    private const int DefaultReprompt = 0;

    public static VaultwardenItemRequest ToItemRequest(this VaultwardenItem source, string name, string username, string password)
    {
        return new VaultwardenItemRequest
        {
            Id = source.Id,
            OrganizationId = source.OrganizationId,
            Type = LoginItemType,
            Name = name,
            Notes = source.Notes,
            Favorite = source.Favorite,
            Reprompt = source.Reprompt,
            Login = new VaultwardenLoginRequest
            {
                Username = username,
                Password = password,
                SecurityAnswers = source.Login?.SecurityAnswers ?? new(),
                Uris = source.Login?.Uris ?? new()
            },
            CollectionIds = source.CollectionIds ?? new()
        };
    }
}
