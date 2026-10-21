using Telegram.Bot;
using Atlas.App.Services;
using Atlas.App.Features.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient("8754079649:AAHzf0tiPWnbfWafweAIOpPzJMaDQzTwX9U"));

builder.Services.AddSingleton<UpdateHandler>();

builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(TextCommand).Assembly));

builder.Services.AddHostedService<BotBackgroundService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
