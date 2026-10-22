using Atlas.Core.Options;
using Atlas.TgBot.Extension.DependencyInjection;
using Atlas.TgBot.Workers;
using Building.Telegram.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<TelegramBotOptions>(
    builder.Configuration.GetSection(TelegramBotOptions.SectionName));

var telegramOptions = builder.Configuration
    .GetSection(TelegramBotOptions.SectionName)
    .Get<TelegramBotOptions>()
    ?? throw new InvalidOperationException($"Секция {TelegramBotOptions.SectionName} не задана");

if (string.IsNullOrWhiteSpace(telegramOptions.BotToken))
{
    throw new InvalidOperationException(
        $"{TelegramBotOptions.SectionName}:{nameof(TelegramBotOptions.BotToken)} не задан");
}

builder.Services.AddTelegramBot(telegramOptions.BotToken);
builder.Services.AddApplication();
builder.Services.AddBotHandlers();
builder.Services.AddHostedService<HeartbeatWorker>();

var app = builder.Build();

app.Run();
