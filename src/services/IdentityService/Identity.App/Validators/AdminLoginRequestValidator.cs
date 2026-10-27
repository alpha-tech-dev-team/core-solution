using FluentValidation;
using Identity.App.Constants;
using Identity.App.Contracts;

namespace Identity.App.Validators;

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
