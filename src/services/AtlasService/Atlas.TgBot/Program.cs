using Atlas.TgBot.Extensions;
using Atlas.TgBot.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAtlasLogging(builder.Configuration);
builder.Services.AddBotInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration, builder.Environment);
builder.Services.AddAtlasBot(builder.Configuration);

var app = builder.Build();

await app.ApplyMigrations();

app.Run();
