namespace CoreSolution.Shared.Abstractions;

public interface IPhoneConfirmationService
{
    Task<string?> ConfirmAsync(long telegramUserId, string? phone, string initialStep, CancellationToken cancellationToken);
}
