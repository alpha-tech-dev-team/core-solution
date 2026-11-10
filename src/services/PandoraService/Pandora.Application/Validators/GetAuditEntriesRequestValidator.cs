using FluentValidation;
using Pandora.Application.Contracts;
using Pandora.Domain.Constants;

namespace Pandora.Application.Validators;

public sealed class GetAuditEntriesRequestValidator : AbstractValidator<GetAuditEntriesRequest>
{
    public GetAuditEntriesRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(PaginationLimits.FirstPage)
            .When(x => x.Page.HasValue);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(PaginationLimits.MinPageSize, PaginationLimits.MaxPageSize)
            .When(x => x.PageSize.HasValue);
    }
}
