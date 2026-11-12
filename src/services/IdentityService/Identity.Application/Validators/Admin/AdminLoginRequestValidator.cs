using FluentValidation;
using Identity.Application.Constants;
using Identity.Application.Contracts.Admin;

namespace Identity.Application.Validators.Admin;

public class AdminLoginRequestValidator : AbstractValidator<AdminLoginRequest>
{
    public AdminLoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MaximumLength(ValidationConstants.AdminUsernameMaxLength);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(ValidationConstants.PasswordMaxLength);
    }
}
