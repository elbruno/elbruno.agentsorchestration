using System.Net.Http.Json;
using System.Text.Json.Serialization;

var cities = new CityInfo[]
{
    new("Toronto", 43.6532, -79.3832),
    new("Tokyo", 35.6762, 139.6503),
    new("Madrid", 40.4168, -3.7038)
};

using var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("WeatherApp/1.0");

Console.WriteLine("🌍 Current Weather Report");
Console.WriteLine(new string('─', 40));

foreach (var city in cities)
{
    try
    {
        var url = $"https://api.open-meteo.com/v1/forecast?latitude={city.Latitude}&longitude={city.Longitude}&current=temperature_2m";
        var response = await httpClient.GetFromJsonAsync<OpenMeteoResponse>(url);
        
        if (response?.Current != null)
        {
            var temp = response.Current.Temperature;
            var emoji = GetWeatherEmoji(temp);
            Console.WriteLine($"{city.Name,-10} {temp,6:F1}°C  {emoji}");
        }
        else
        {
            Console.WriteLine($"{city.Name,-10} Unable to fetch weather data");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{city.Name,-10} Error: {ex.Message}");
    }
}

Console.WriteLine(new string('─', 40));

static string GetWeatherEmoji(double temperature) => temperature switch
{
    >= 25 => "☀️",   // Warm (25°C+)
    >= 10 => "🌤️",  // Mild (10-24°C)
    _ => "❄️"        // Cold (below 10°C)
};

sealed record CityInfo(string Name, double Latitude, double Longitude);

sealed record OpenMeteoResponse
{
    [JsonPropertyName("current")]
    public CurrentWeather? Current { get; init; }
}

sealed record CurrentWeather
{
    [JsonPropertyName("temperature_2m")]
    public double Temperature { get; init; }
}
