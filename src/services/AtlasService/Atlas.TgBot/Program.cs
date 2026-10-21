using Atlas.TgBot.Constants;
using Atlas.TgBot.Extension.DependencyInjection;
using Building.Telegram.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTelegramBot(builder.Configuration[ConfigurationKeys.TelegramBotToken]
    ?? throw new InvalidOperationException("Не задан Telegram:BotToken"));
builder.Services.AddApplication();
builder.Services.AddBotHandlers();

var app = builder.Build();

app.Run();
