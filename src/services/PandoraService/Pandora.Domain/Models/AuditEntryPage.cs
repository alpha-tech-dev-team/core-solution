using Pandora.Domain.Entities;

namespace Pandora.Domain.Models;

public sealed record AuditEntryPage(IReadOnlyList<AuditEntry> Items, int Page, int PageSize, int Total);
