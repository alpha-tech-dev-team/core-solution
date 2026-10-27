using FluentValidation;
using Identity.App.Constants;
using Identity.App.Contracts;

namespace Identity.App.Validators;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .MaximumLength(ValidationConstants.RefreshTokenMaxLength);
    }
}
