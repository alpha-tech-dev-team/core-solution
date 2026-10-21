using Building.Telegram.DependencyInjection;
using Pandora.TgBot.Extension.DI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddTelegramBot(builder.Configuration["Telegram:BotToken"] ?? string.Empty);
builder.Services.AddApplication();
builder.Services.AddBotHandlers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();
