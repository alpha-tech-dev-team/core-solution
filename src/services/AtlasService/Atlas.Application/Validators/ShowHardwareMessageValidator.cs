using Atlas.Application.Features.Hardware;
using Atlas.Domain.Constants;
using Atlas.Domain.Enums;
using FluentValidation;

namespace Atlas.Application.Validators;

public sealed class ShowHardwareMessageValidator : AbstractValidator<ShowHardwareMessage>
{
    public ShowHardwareMessageValidator()
    {
        RuleFor(message => message.TelegramUserId).GreaterThan(ValidationConstants.TelegramUserIdMinExclusive).WithErrorCode(nameof(ErrorCode.InvalidTelegramUserId));
        RuleFor(message => message.MessageId).GreaterThan(ValidationConstants.MessageIdMinExclusive).WithErrorCode(nameof(ErrorCode.InvalidMessageId));
        RuleFor(message => message.Page).GreaterThanOrEqualTo(ValidationConstants.PageMinInclusive).WithErrorCode(nameof(ErrorCode.InvalidPageNumber));
    }
}
