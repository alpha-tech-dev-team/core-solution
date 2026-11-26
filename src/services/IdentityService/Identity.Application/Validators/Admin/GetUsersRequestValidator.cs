using CoreSolution.Shared.Application.Validation;
using FluentValidation;
using Identity.Application.Constants;
using Identity.Application.Contracts.Admin;
using Identity.Application.Features.Admin;

namespace Identity.Application.Validators.Admin;

public class GetUsersRequestValidator : AbstractValidator<GetUsersRequest>
{
    public GetUsersRequestValidator()
    {
        this.ApplyPaginationRules();

        // Общий диапазон шире, но API выдачи сотрудников сохраняет собственный предел страницы.
        RuleFor(x => x.PageSize)
            .LessThanOrEqualTo(ValidationConstants.PageSizeMax);

        RuleFor(x => x.SortBy)
            .Must(EmployeeSortFields.IsSupported)
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy));

        RuleFor(x => x.SortDirection)
            .Must(direction => direction is null || Enum.IsDefined(direction.Value));
    }
}
