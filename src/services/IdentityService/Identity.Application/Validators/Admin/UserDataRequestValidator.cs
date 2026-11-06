using FluentValidation;
using Identity.Application.Constants;
using Identity.Application.Contracts.Admin;

namespace Identity.Application.Validators.Admin;

public class UserDataRequestValidator : AbstractValidator<UserDataRequest>
{
    public UserDataRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(ValidationConstants.PersonNameMaxLength);

        RuleFor(x => x.Surname)
            .NotEmpty()
            .MaximumLength(ValidationConstants.PersonNameMaxLength);

        RuleFor(x => x.Patronymic)
            .MaximumLength(ValidationConstants.PersonNameMaxLength);

        RuleFor(x => x.Position)
            .NotEmpty()
            .MaximumLength(ValidationConstants.PositionMaxLength);

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(ValidationConstants.PhoneMaxLength);
    }
}
