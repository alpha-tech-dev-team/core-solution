using Atlas.Infrastructure.Options;
using Atlas.TgBot.Extensions.DependencyInjection;
using Building.Telegram.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

app.Run();
