using Pandora.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceConfiguration(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapRouteConfiguration();

app.Run();
