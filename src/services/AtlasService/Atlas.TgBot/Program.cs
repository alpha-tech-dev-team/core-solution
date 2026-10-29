using Atlas.TgBot.Extensions.DependencyInjection;
using Building.Telegram.DependencyInjection;
using Building.Telegram.Internal;

var builder = WebApplication.CreateBuilder(args);


var telegramOptions = builder.Services.AddAndValidateTelegramOptions(builder.Configuration);
builder.Services.AddTelegramBot(telegramOptions.BotToken);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddBotHandlers();
builder.Services.AddHostedService<BotWorker>();

var app = builder.Build();

app.Run();
