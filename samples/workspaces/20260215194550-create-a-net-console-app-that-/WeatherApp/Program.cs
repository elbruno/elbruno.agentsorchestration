using System.Net.Http.Json;
using System.Text.Json.Serialization;

// City coordinates for weather lookup
var cities = new (string Name, double Latitude, double Longitude)[]
{
    ("Toronto", 43.6532, -79.3832),
    ("Tokyo", 35.6762, 139.6503),
    ("Madrid", 40.4168, -3.7038)
};

using var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Add("User-Agent", "WeatherApp/1.0");

Console.WriteLine("🌍 Current Weather Report");
Console.WriteLine(new string('═', 40));

foreach (var city in cities)
{
    try
    {
        var url = $"https://api.open-meteo.com/v1/forecast?latitude={city.Latitude}&longitude={city.Longitude}&current=temperature_2m";
        var response = await httpClient.GetFromJsonAsync<OpenMeteoResponse>(url);

        if (response?.Current?.Temperature is { } temp)
        {
            var emoji = GetWeatherEmoji(temp);
            Console.WriteLine($"{emoji} {city.Name}: {temp:F1}°C");
        }
        else
        {
            Console.WriteLine($"❓ {city.Name}: Unable to retrieve temperature");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ {city.Name}: Error - {ex.Message}");
    }
}

Console.WriteLine(new string('═', 40));

static string GetWeatherEmoji(double temperature) => temperature switch
{
    >= 25 => "☀️",  // Warm (25°C and above)
    >= 10 => "🌤️", // Mild (10-24°C)
    _ => "❄️"       // Cold (below 10°C)
};

// Open-Meteo API response models
public sealed record OpenMeteoResponse(
    [property: JsonPropertyName("current")] CurrentWeather? Current
);

public sealed record CurrentWeather(
    [property: JsonPropertyName("temperature_2m")] double? Temperature
);
