using Atlas.Infrastructure.DependencyInjection;
using Atlas.Infrastructure.Options;
using Atlas.TgBot.Extensions.DependencyInjection;
using Building.Telegram.DependencyInjection;
using Building.Telegram.Internal;

var builder = WebApplication.CreateBuilder(args);


var telegramOptions = builder.Configuration
    .GetSection(TelegramBotOptions.SectionName)
    .Get<TelegramBotOptions>()
    ?? throw new InvalidOperationException($"Section {TelegramBotOptions.SectionName} is missing.");

if (string.IsNullOrWhiteSpace(telegramOptions.BotToken))
{
    throw new InvalidOperationException($"{TelegramBotOptions.SectionName}:{nameof(TelegramBotOptions.BotToken)} is missing.");
}

builder.Services.AddTelegramBot(telegramOptions.BotToken);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddBotHandlers();
builder.Services.AddHostedService<BotWorker>();

var app = builder.Build();

app.Run();
