using FluentValidation;
using Identity.Application.Constants;
using Identity.Application.Contracts.Admin;

namespace Identity.Application.Validators.Admin;

public class SetUserRolesRequestValidator : AbstractValidator<SetUserRolesRequest>
{
    public SetUserRolesRequestValidator()
    {
        RuleFor(x => x.Roles)
            .NotNull();

        RuleForEach(x => x.Roles)
            .NotEmpty()
            .MaximumLength(ValidationConstants.AccessMaxLength);
    }
}
