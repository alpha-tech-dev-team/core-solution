using Atlas.TgBot;
using Atlas.TgBot.Extensions.DependencyInjection;
using Building.Telegram.DependencyInjection;
using Building.Telegram.Internal;
using Atlas.Infrastructure.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAtlasOptions(builder.Configuration);
builder.Services.AddTelegramBot(builder.Configuration[ConfigurationKeys.TelegramBotToken] 
    ?? string.Empty);
builder.Services.AddApplication();
builder.Services.AddBotHandlers();
builder.Services.AddHostedService<BotWorker>();

var app = builder.Build();

app.Run();
