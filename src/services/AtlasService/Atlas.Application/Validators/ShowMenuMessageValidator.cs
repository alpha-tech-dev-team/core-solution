using Atlas.Application.Features.Menu;
using Atlas.Domain.Constants;
using Atlas.Domain.Enums;
using CoreSolution.Shared.Application.Validation;
using FluentValidation;

namespace Atlas.Application.Validators;

public sealed class ShowMenuMessageValidator : AbstractValidator<ShowMenuMessage>
{
    public ShowMenuMessageValidator()
    {
        RuleFor(message => message.TelegramUserId)
            .GreaterThan(ValidationConstants.TelegramUserIdMinExclusive)
            .WithErrorCode(nameof(ErrorCode.InvalidTelegramUserId));

        RuleFor(message => message.MessageId)
            .GreaterThan(ValidationConstants.MessageIdMinExclusive)
            .WithErrorCode(nameof(ErrorCode.InvalidMessageId));

        this.ApplyPaginationRules(nameof(ErrorCode.InvalidPageNumber));
    }
}
