using CoreSolution.Shared.Constants;
using FluentValidation;
using Pandora.Application.Features.Audit;
using Pandora.Domain.Enums;

namespace Pandora.Application.Validators;

public sealed class GetAuditEntriesMessageValidator : AbstractValidator<GetAuditEntriesMessage>
{
    public GetAuditEntriesMessageValidator()
    {
        RuleFor(message => message.Page)
            .GreaterThanOrEqualTo(PaginationLimits.FirstPage)
            .WithErrorCode(nameof(ErrorCode.InvalidPage))
            .When(message => message.Page.HasValue);

        RuleFor(message => message.PageSize)
            .InclusiveBetween(PaginationLimits.MinPageSize, PaginationLimits.MaxPageSize)
            .WithErrorCode(nameof(ErrorCode.InvalidPageSize))
            .When(message => message.PageSize.HasValue);
    }
}
