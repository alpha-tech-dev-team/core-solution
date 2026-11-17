using FluentValidation;
using Pandora.Application.Features.Audit;
using Pandora.Domain.Models;
using Pandora.Application.Validation;

namespace Pandora.Application.Validators;

public sealed class GetAuditEntriesMessageValidator : AbstractValidator<GetAuditEntriesMessage>
{
    public GetAuditEntriesMessageValidator()
    {
        this.AddPagedSearchSortRules(AuditEntrySortFields.Fields.Keys.ToArray());
    }
}
