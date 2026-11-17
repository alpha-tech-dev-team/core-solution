using CoreSolution.Shared.Constants;
using FluentValidation;
using Pandora.Application.Features.Audit;
using Pandora.Domain.Enums;
using Pandora.Domain.Models;

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

        RuleFor(message => message.SortBy)
            .Must(field => AuditEntrySortFields.Fields.ContainsKey(field!))
            .WithErrorCode(nameof(ErrorCode.InvalidSortField))
            .When(message => message.SortBy is not null);

        RuleFor(message => message.SortDirection)
            .Must(direction => string.Equals(direction, "asc", StringComparison.OrdinalIgnoreCase)
                || string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase))
            .WithErrorCode(nameof(ErrorCode.InvalidSortDirection))
            .When(message => message.SortDirection is not null);
    }
}
