using Atlas.Application.Features.Software;
using Atlas.Domain.Constants;
using Atlas.Domain.Enums;
using FluentValidation;

namespace Atlas.Application.Validators;

public sealed class ShowSoftwareMessageValidator : AbstractValidator<ShowSoftwareMessage>
{
    public ShowSoftwareMessageValidator()
    {
        RuleFor(message => message.TelegramUserId).GreaterThan(ValidationConstants.TelegramUserIdMinExclusive).WithErrorCode(nameof(ErrorCode.InvalidTelegramUserId));
        RuleFor(message => message.MessageId).GreaterThan(ValidationConstants.MessageIdMinExclusive).WithErrorCode(nameof(ErrorCode.InvalidMessageId));
        RuleFor(message => message.Page).GreaterThanOrEqualTo(ValidationConstants.PageMinInclusive).WithErrorCode(nameof(ErrorCode.InvalidPageNumber));
    }
}
