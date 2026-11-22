using Atlas.Application.Features.Incidents;
using CoreSolution.Shared.Application.Validation;
using FluentValidation;

namespace Atlas.Application.Validators;

public sealed class GetIncidentsMessageValidator : AbstractValidator<GetIncidentsMessage>
{
    public GetIncidentsMessageValidator()
    {
        this.ApplyPaginationRules();
    }
}
