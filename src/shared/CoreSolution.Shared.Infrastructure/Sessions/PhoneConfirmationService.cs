using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;

namespace CoreSolution.Shared.Infrastructure.Sessions;

public sealed class PhoneConfirmationService : IPhoneConfirmationService
{
    private const string InternationalPrefix = "+";

    private readonly IIdentityClient _identityClient;
    private readonly ISessionService _sessionService;

    public PhoneConfirmationService(IIdentityClient identityClient, ISessionService sessionService, bool requiresPhone)
    {
        _identityClient = identityClient;
        _sessionService = sessionService;
        RequiresPhone = requiresPhone;
    }

    public bool RequiresPhone { get; }

    public async Task<bool> ConfirmAsync(long telegramUserId, string? phone, string initialStep, CancellationToken cancellationToken)
    {
        if (RequiresPhone && phone is not null)
        {
            var approved = await _identityClient.ActivateUser(telegramUserId, NormalizePhone(phone), cancellationToken);

            if (!approved)
            {
                return false;
            }
        }

        await _sessionService.SaveSession(UserSession.Create(telegramUserId, initialStep), cancellationToken);

        return true;
    }

    private static string NormalizePhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        return $"{InternationalPrefix}{digits}";
    }
}
