using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Enums;
using CoreSolution.Shared.Models;

namespace CoreSolution.Shared.Infrastructure.Sessions;

public sealed class PhoneConfirmationService : IPhoneConfirmationService
{
    private const string InternationalPrefix = "+";

    private readonly IIdentityClient _identityClient;
    private readonly ISessionService _sessionService;

    public PhoneConfirmationService(IIdentityClient identityClient, ISessionService sessionService)
    {
        _identityClient = identityClient;
        _sessionService = sessionService;
    }

    public async Task<PhoneConfirmationResult> ConfirmAsync(long telegramUserId, string? phone, string initialStep, CancellationToken cancellationToken)
    {
        if (phone is null)
        {
            return PhoneConfirmationResult.PhoneRequired;
        }

        var approved = await _identityClient.ActivateUser(telegramUserId, NormalizePhone(phone), cancellationToken);

        if (!approved)
        {
            return PhoneConfirmationResult.AccessDenied;
        }

        await _sessionService.SaveSession(UserSession.Create(telegramUserId, initialStep), cancellationToken);

        return PhoneConfirmationResult.Confirmed;
    }

    private static string NormalizePhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        return $"{InternationalPrefix}{digits}";
    }
}
