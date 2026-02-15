using System.Net.Http.Json;
using WeatherApp;
using WeatherApp.Models;

// Define cities with their coordinates
var cities = new CityLocation[]
{
    new("Toronto", 43.6532, -79.3832),
    new("Tokyo", 35.6762, 139.6503),
    new("Madrid", 40.4168, -3.7038)
};

// Print header
ConsoleStyles.PrintHeader("🌍 Weather Report");

using var httpClient = new HttpClient();

foreach (var city in cities)
{
    try
    {
        // Open-Meteo API - free, no API key required
        var url = $"https://api.open-meteo.com/v1/forecast?latitude={city.Latitude}&longitude={city.Longitude}&current=temperature_2m,weather_code";
        var response = await httpClient.GetFromJsonAsync<WeatherApiResponse>(url);
        
        if (response?.Current is not null)
        {
            ConsoleStyles.PrintWeatherCard(city.Name, response.Current.Temperature);
        }
        else
        {
            Console.WriteLine($"  {city.Name}: Unable to retrieve weather data");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  {city.Name}: Error - {ex.Message}");
    }
}

ConsoleStyles.PrintFooter();