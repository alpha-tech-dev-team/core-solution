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
            .MinimumLength(ValidationConstants.PersonNameMinLength)
            .MaximumLength(ValidationConstants.PersonNameMaxLength)
            .Matches(ValidationConstants.PersonNamePattern);

        RuleFor(x => x.Surname)
            .NotEmpty()
            .MinimumLength(ValidationConstants.PersonNameMinLength)
            .MaximumLength(ValidationConstants.PersonNameMaxLength)
            .Matches(ValidationConstants.PersonNamePattern);

        RuleFor(x => x.Patronymic)
            .MinimumLength(ValidationConstants.PersonNameMinLength)
            .MaximumLength(ValidationConstants.PersonNameMaxLength)
            .Matches(ValidationConstants.PersonNamePattern)
            .When(x => !string.IsNullOrWhiteSpace(x.Patronymic));

        RuleFor(x => x.Position)
            .NotEmpty()
            .MinimumLength(ValidationConstants.PositionMinLength)
            .MaximumLength(ValidationConstants.PositionMaxLength)
            .Matches(ValidationConstants.PositionPattern);

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(ValidationConstants.PhoneMaxLength)
            .Matches(ValidationConstants.PhonePattern);
    }
}
