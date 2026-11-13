using CoreSolution.Shared.Enums;

namespace CoreSolution.Shared.Abstractions;

public interface IPhoneConfirmationService
{
    Task<PhoneConfirmationResult> ConfirmAsync(long telegramUserId, string? phone, string initialStep, CancellationToken cancellationToken);
}
