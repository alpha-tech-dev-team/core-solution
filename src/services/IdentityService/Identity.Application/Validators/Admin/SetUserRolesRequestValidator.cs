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
            .IsInEnum();

        RuleFor(x => x.AdminLogin)
            .MaximumLength(ValidationConstants.AdminUsernameMaxLength)
            .When(x => x.AdminLogin is not null);

        RuleFor(x => x.AdminPassword)
            .MaximumLength(ValidationConstants.PasswordMaxLength)
            .When(x => x.AdminPassword is not null);
    }
}
