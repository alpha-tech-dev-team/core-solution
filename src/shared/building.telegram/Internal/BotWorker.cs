using Building.Telegram.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Building.Telegram.Internal;

public sealed class BotWorker : BackgroundService
{
    private readonly ITelegramBotClient _client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BotWorker> _logger;

    public BotWorker(
        ITelegramBotClient client,
        IServiceScopeFactory scopeFactory,
        ILogger<BotWorker> logger)
    {
        _client = client;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery],
            DropPendingUpdates = true
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _client.ReceiveAsync(
                    updateHandler: HandleUpdateAsync,
                    errorHandler: HandleErrorAsync,
                    receiverOptions: receiverOptions,
                    cancellationToken: stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Long polling завершился с ошибкой");
            }

            try
            {
                await Task.Delay(TelegramConstants.PollingErrorDelay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<IBotUpdateHandler>();

        try
        {
            await handler.HandleUpdateAsync(update, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка обработки апдейта {UpdateId}", update.Id);
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        _logger.LogError(exception, "Ошибка long polling");

        return Task.CompletedTask;
    }
}
