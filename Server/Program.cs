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

app.MapGet("/weatherforecast-or-sportscore", async (HttpContext ctx) =>
{
    ctx.Response.Headers.Append(HeaderNames.ContentType, "text/event-stream");
    while (!ctx.RequestAborted.IsCancellationRequested)
    {
        if (Random.Shared.Next(2) == 0)
        {
            var forecast = new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(Random.Shared.Next(8))),
                    Random.Shared.Next(-40, 50),
                    summaries[Random.Shared.Next(summaries.Length)]
                );

            await WriteEvent(ctx, "WeatherForecast", forecast);
        } 
        else
        {
            var score = new SportScore(Random.Shared.Next(10), Random.Shared.Next(10));
            await WriteEvent(ctx, "SportScore", score);
        }

        await Task.Delay(2000);
    }
});

async Task WriteEvent<T>(HttpContext ctx, string eventName, T data)
{
    await ctx.Response.WriteAsync($"event: {eventName}\n");
    await ctx.Response.WriteAsync($"data: ");
    await JsonSerializer.SerializeAsync(ctx.Response.Body, data);
    await ctx.Response.WriteAsync($"\n\n");
    await ctx.Response.Body.FlushAsync();
}

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary);

record SportScore(int Team1Score, int Team2Score);
