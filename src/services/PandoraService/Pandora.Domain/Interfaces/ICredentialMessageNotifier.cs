namespace Pandora.Domain.Interfaces;

public interface ICredentialMessageNotifier
{
    Task SendNotice(long chatId, string text, CancellationToken cancellationToken);
}
