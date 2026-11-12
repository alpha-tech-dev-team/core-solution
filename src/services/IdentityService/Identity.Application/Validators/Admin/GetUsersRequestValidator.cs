using FluentValidation;
using Identity.Application.Constants;
using Identity.Application.Contracts.Admin;

namespace Identity.Application.Validators.Admin;

public class GetUsersRequestValidator : AbstractValidator<GetUsersRequest>
{
    public GetUsersRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(ValidationConstants.PageMin);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(ValidationConstants.PageSizeMin, ValidationConstants.PageSizeMax);
    }
}
