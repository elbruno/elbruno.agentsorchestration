using System.Net.Http.Json;
using System.Text.Json.Serialization;

// City coordinates for weather lookup
var cities = new (string Name, double Latitude, double Longitude)[]
{
    ("Toronto", 43.70, -79.42),
    ("Tokyo", 35.69, 139.69),
    ("Madrid", 40.42, -3.70)
};

Console.WriteLine("🌍 Current Weather Report");
Console.WriteLine(new string('=', 40));

using var httpClient = new HttpClient();

foreach (var city in cities)
{
    try
    {
        var url = $"https://api.open-meteo.com/v1/forecast?latitude={city.Latitude}&longitude={city.Longitude}&current=temperature_2m";
        var response = await httpClient.GetFromJsonAsync<WeatherResponse>(url);
        
        if (response?.Current != null)
        {
            var temp = response.Current.Temperature;
            var emoji = GetWeatherEmoji(temp);
            Console.WriteLine($"{city.Name,-10} {temp,5:F1}°C  {emoji}");
        }
        else
        {
            Console.WriteLine($"{city.Name,-10} Unable to retrieve weather data");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{city.Name,-10} Error: {ex.Message}");
    }
}

Console.WriteLine(new string('=', 40));

static string GetWeatherEmoji(double temperature) => temperature switch
{
    >= 25 => "☀️",  // Warm
    >= 10 => "🌤️", // Mild
    _ => "❄️"       // Cold
};

public sealed class WeatherResponse
{
    [JsonPropertyName("current")]
    public CurrentWeather? Current { get; set; }
}

public sealed class CurrentWeather
{
    [JsonPropertyName("temperature_2m")]
    public double Temperature { get; set; }
}
