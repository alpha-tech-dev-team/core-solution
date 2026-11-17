using CoreSolution.Shared.Pagination;
using Pandora.Domain.Entities;

namespace Pandora.Domain.Models;

public sealed record SecretPageMessage(
    PageRequest Page,
    Func<IQueryable<SecretMetadata>, IOrderedQueryable<SecretMetadata>> OrderBy);
