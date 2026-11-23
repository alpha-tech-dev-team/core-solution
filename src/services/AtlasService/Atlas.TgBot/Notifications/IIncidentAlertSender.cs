namespace Atlas.TgBot.Notifications;

public interface IIncidentAlertSender
{
    Task SendAlertAsync(long chatId, string text, CancellationToken cancellationToken = default);
}
