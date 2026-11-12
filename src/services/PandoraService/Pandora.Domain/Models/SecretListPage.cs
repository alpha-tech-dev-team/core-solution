namespace Pandora.Domain.Models;

public sealed record SecretListPage(IReadOnlyList<SecretListItem> Items, int Page, int PageSize, int Total);
