using FluentValidation;
using Pandora.Application.Features.Audit;
using Pandora.Domain.Constants;

namespace Pandora.Application.Validators;

public sealed class GetAuditEntriesMessageValidator : AbstractValidator<GetAuditEntriesMessage>
{
    public GetAuditEntriesMessageValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(PaginationLimits.DefaultPage)
            .When(x => x.Page.HasValue);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(PaginationLimits.MinPageSize, PaginationLimits.MaxPageSize)
            .When(x => x.PageSize.HasValue);
    }
}
