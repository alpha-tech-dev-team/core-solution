using CoreSolution.Shared.Models;

namespace Atlas.App.Features.Activation;

public sealed class SubmitPhoneResponse
{
    public TelegramMessageContent MenuContent { get; init; } = null!;
}
