using Atlas.Infrastructure.Options;
using Atlas.TgBot.Extensions.DependencyInjection;
using Building.Telegram.Internal;
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
builder.Services.Configure<Atlas.Infrastructure.Options.AccessOptions>(
    builder.Configuration.GetSection("AccessOptions"));

builder.Services.AddScoped<Atlas.Core.Interfaces.IAccessService, Atlas.Infrastructure.Access.AccessService>();
builder.Services.AddTelegramBot(telegramOptions.BotToken);
builder.Services.AddApplication();
builder.Services.AddBotHandlers();
builder.Services.AddHostedService<BotWorker>();

var app = builder.Build();

app.Run();