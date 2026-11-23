using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.Internal.UpdatePipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CoreSolution.Shared.TgBot.Internal.Hosting;

public sealed class BotWorker : BackgroundService
{
    private static readonly TimeSpan PollingErrorDelay = TimeSpan.FromSeconds(5);

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
                await Task.Delay(PollingErrorDelay, stoppingToken);
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

        try
        {
            await SeedSessionAsync(scope.ServiceProvider, update, cancellationToken);

            var handler = scope.ServiceProvider.GetRequiredService<IBotUpdateHandler>();

            await handler.HandleUpdateAsync(update, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            var (tgUserId, messageId) = UpdateContext.Extract(update);

            _logger.LogError(ex, "Ошибка обработки апдейта {UpdateId}, пользователь {TgUserId}, сообщение {MessageId}", update.Id, tgUserId, messageId);
        }
    }

    private static async Task SeedSessionAsync(IServiceProvider services, Update update, CancellationToken cancellationToken)
    {
        if (services.GetService<ISessionService>() is not { } sessionService
            || services.GetService<IRequestContext>() is not { } requestContext)
        {
            return;
        }

        var (tgUserId, _) = UpdateContext.Extract(update);

        if (tgUserId is not long userId)
        {
            return;
        }

        var session = await sessionService.GetSession(userId, cancellationToken);

        if (session is not null)
        {
            requestContext.Session = session;
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
