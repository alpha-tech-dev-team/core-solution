using Atlas.Application.Features.Incidents;
using Atlas.Domain.Enums;
using CoreSolution.Shared.Constants;
using FluentValidation;

namespace Atlas.Application.Validators;

public sealed class GetIncidentsMessageValidator : AbstractValidator<GetIncidentsMessage>
{
    public GetIncidentsMessageValidator()
    {
        RuleFor(message => message.Page)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .GreaterThanOrEqualTo(PaginationLimits.FirstPage)
            .WithErrorCode(nameof(ErrorCode.ValidationFailed));

        RuleFor(message => message.PageSize)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .InclusiveBetween(PaginationLimits.MinPageSize, PaginationLimits.MaxPageSize)
            .WithErrorCode(nameof(ErrorCode.ValidationFailed));
    }
}
