using Atlas.Infrastructure.DependencyInjection;
using Atlas.TgBot.Extensions.DependencyInjection;
using Building.Telegram.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTelegramOptions(builder.Configuration);
builder.Services.AddAccessControl(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddBotHandlers();

var app = builder.Build();
app.Run();