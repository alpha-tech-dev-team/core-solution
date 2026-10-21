using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace Atlas.App.Services;

public class BotBackgroundService : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly UpdateHandler _updateHandler;

    public BotBackgroundService(ITelegramBotClient botClient, UpdateHandler updateHandler)
    {
        _botClient = botClient;
        _updateHandler = updateHandler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = []
        };

        _botClient.StartReceiving(
            updateHandler: _updateHandler.HandleUpdateAsync, 
            errorHandler: _updateHandler.HandleErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken
        );

        Console.WriteLine("Telegram-бот Long Polling");
        
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
