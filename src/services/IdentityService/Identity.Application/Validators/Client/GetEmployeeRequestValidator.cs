using FluentValidation;
using Identity.Application.Constants;
using Identity.Application.Contracts.Client;

namespace Identity.Application.Validators.Client;

public class GetEmployeeRequestValidator : AbstractValidator<GetEmployeeRequest>
{
    public GetEmployeeRequestValidator()
    {
        RuleFor(x => x.TelegramUserId)
            .GreaterThan(ValidationConstants.TelegramUserIdMinExclusive)
            .LessThanOrEqualTo(ValidationConstants.TelegramUserIdMaxInclusive);
    }
}
