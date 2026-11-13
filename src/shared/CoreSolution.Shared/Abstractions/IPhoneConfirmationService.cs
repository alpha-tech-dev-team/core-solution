namespace CoreSolution.Shared.Abstractions;

public interface IPhoneConfirmationService
{
    Task<bool> ConfirmAsync(long telegramUserId, string? phone, string initialStep, CancellationToken cancellationToken);
}
