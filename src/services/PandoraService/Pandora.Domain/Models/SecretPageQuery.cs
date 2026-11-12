using Pandora.Domain.Entities;

namespace Pandora.Domain.Models;

public sealed record SecretPageQuery(
    int Page,
    int PageSize,
    Func<IQueryable<SecretMetadata>, IOrderedQueryable<SecretMetadata>> OrderBy);
