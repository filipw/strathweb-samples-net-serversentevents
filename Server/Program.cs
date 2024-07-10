using Microsoft.Net.Http.Headers;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", async (HttpContext ctx) =>
{
    ctx.Response.Headers.Append(HeaderNames.ContentType, "text/event-stream");
    while (!ctx.RequestAborted.IsCancellationRequested)
    {
        var forecast = new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(Random.Shared.Next(0, 8))),
                Random.Shared.Next(-40, 50),
                summaries[Random.Shared.Next(summaries.Length)]
            );

        await ctx.Response.WriteAsync($"data: ");
        await JsonSerializer.SerializeAsync(ctx.Response.Body, forecast);
        await ctx.Response.WriteAsync($"\n\n");
        await ctx.Response.Body.FlushAsync();

        await Task.Delay(2000);
    }
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary);
