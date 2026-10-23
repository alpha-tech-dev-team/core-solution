using Atlas.Infrastructure.Options;
using Atlas.TgBot.Extensions.DependencyInjection;
using Building.Telegram.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var telegramOptions = builder.Configuration.GetTelegramOptions();

builder.Services.AddTelegramBot(telegramOptions.BotToken);
builder.Services.AddApplication();
builder.Services.AddBotHandlers();

var app = builder.Build();

app.Run();
