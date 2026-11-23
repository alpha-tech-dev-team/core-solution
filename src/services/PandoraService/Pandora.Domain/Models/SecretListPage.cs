namespace Pandora.Domain.Models;

public sealed record SecretListPage(IReadOnlyList<SecretPageItem> Items, int Page, int PageSize, int Total);
