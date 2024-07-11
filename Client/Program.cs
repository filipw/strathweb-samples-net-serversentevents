using System.Net.ServerSentEvents;
using System.Text.Json;

//await Example1String();
//await Example2TypedSingleEventType();
await Example3TypedMultipleEventType();

async Task Example1String()
{
    using var client = new HttpClient();
    using var stream = await client.GetStreamAsync("http://localhost:5068/weatherforecast");
    await foreach (SseItem<string> item in SseParser.Create(stream).EnumerateAsync())
    {
        Console.WriteLine(item.Data);
    }
}

async Task Example2TypedSingleEventType()
{
    using var client = new HttpClient();
    using var stream = await client.GetStreamAsync("http://localhost:5068/weatherforecast");
    await foreach (SseItem<WeatherForecast?> item in SseParser.Create(stream, (eventType, bytes) => 
        JsonSerializer.Deserialize<WeatherForecast>(bytes)).EnumerateAsync())
    {
        if (item.Data != null)
        {
            Console.WriteLine($"Date: {item.Data.Date}, Temperature (in C): {item.Data.TemperatureC}, Summary: {item.Data.Summary}");
        }
        else
        {
            Console.WriteLine("Couldn't deserialize the response");
        }
    }
}

async Task Example3TypedMultipleEventType()
{
    using var client = new HttpClient();
    using var stream = await client.GetStreamAsync("http://localhost:5068/weatherforecast-or-sportscore");
    await foreach (var item in SseParser.Create(stream, (eventType, bytes) => eventType switch
    {
        "WeatherForecast" => JsonSerializer.Deserialize<WeatherForecast>(bytes),
        "SportScore" => JsonSerializer.Deserialize<SportScore>(bytes) as object,
        _ => null
    }).EnumerateAsync())
    {
        switch (item.Data)
        {
            case WeatherForecast weatherForecast:
                Console.WriteLine($"Date: {weatherForecast.Date}, Temperature (in C): {weatherForecast.TemperatureC}, Summary: {weatherForecast.Summary}");
                break;
            case SportScore sportScore:
                Console.WriteLine($"Team 1 vs Team 2 {sportScore.Team1Score}:{sportScore.Team2Score}");
                break;
            default:
                Console.WriteLine("Couldn't deserialize the response");
                break;
        }
    }
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary);
record SportScore(int Team1Score, int Team2Score);