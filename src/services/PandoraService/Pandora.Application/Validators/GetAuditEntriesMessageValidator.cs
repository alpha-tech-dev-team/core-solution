using FluentValidation;
using Pandora.Application.Features.Audit;
using Pandora.Domain.Models;
using Pandora.Application.Validation;
using Pandora.Domain.Constants;
using Pandora.Domain.Enums;

namespace Pandora.Application.Validators;

public sealed class GetAuditEntriesMessageValidator : AbstractValidator<GetAuditEntriesMessage>
{
    public GetAuditEntriesMessageValidator()
    {
        this.AddPagedSearchSortRules(AuditEntrySortFields.Fields.Keys.ToArray());

        RuleFor(message => message.Search)
            .MaximumLength(AuditConstants.SearchMaxLength)
            .WithErrorCode(nameof(ErrorCode.InvalidSearch));
    }
}
