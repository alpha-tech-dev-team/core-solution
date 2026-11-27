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
        this.ApplyPaginationRules(maxPageSize: ValidationConstants.PageSizeMax);
        this.ApplySortRules(EmployeeSortFields.Instance.SupportedFields);
    }
}
