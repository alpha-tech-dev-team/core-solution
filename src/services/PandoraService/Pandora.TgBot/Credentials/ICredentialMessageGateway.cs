using Pandora.Domain.Enums;

namespace Pandora.TgBot.Credentials;

public interface ICredentialMessageGateway
{
    Task<CredentialMessageOutcome> Delete(long chatId, int messageId, CancellationToken cancellationToken);
    Task<CredentialMessageOutcome> ReplaceText(
        long chatId,
        int messageId,
        string text,
        CancellationToken cancellationToken);
}
