using System.Net.ServerSentEvents;
using System.Text.Json;

using var client = new HttpClient();
using var stream = await client.GetStreamAsync("http://localhost:5068/weatherforecast");

// option 1: as string
//await foreach (SseItem<string> item in SseParser.Create(stream).EnumerateAsync())
//{
//    Console.WriteLine(item.Data);
//}

// option 2: typed
await foreach (SseItem<WeatherForecast?> item in SseParser.Create(stream, (eventType, bytes) => JsonSerializer.Deserialize<WeatherForecast>(bytes)).EnumerateAsync())
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

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary);