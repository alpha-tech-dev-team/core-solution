using Identity.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceConfiguration(builder.Configuration);

var app = builder.Build();

app.UseMiddlewareConfiguration();
await app.AddApplicationConfiguration();
app.MapRouteConfiguration();

app.Run();
