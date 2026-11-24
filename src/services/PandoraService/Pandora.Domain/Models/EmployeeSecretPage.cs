namespace Pandora.Domain.Models;

public sealed record EmployeeSecretPage(IReadOnlyList<EmployeeSecretItem> Items, int Page, int PageSize, int Total);
