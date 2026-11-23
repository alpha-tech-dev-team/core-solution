using CoreSolution.Shared.Models;
using Pandora.Domain.Entities;

namespace Pandora.Domain.Models;

public sealed record SecretPageMessage(
    PagedMessage Paging,
    Func<IQueryable<SecretMetadata>, IOrderedQueryable<SecretMetadata>> OrderBy);
