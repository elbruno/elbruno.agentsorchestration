using System.Net.Http.Json;
using WeatherApp;
using WeatherApp.Models;

// Configure HTTP client for Open-Meteo API
using var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://api.open-meteo.com/")
};

ConsoleStyles.PrintHeader();

foreach (var city in CityCoordinates.AllCities)
{
    try
    {
        var url = $"v1/forecast?latitude={city.Latitude}&longitude={city.Longitude}&current_weather=true";
        var response = await httpClient.GetFromJsonAsync<OpenMeteoResponse>(url);
        
        if (response?.CurrentWeather != null)
        {
            ConsoleStyles.PrintCityWeather(city.Name, response.CurrentWeather.Temperature);
        }
        else
        {
            ConsoleStyles.PrintError($"No weather data for {city.Name}");
        }
    }
    catch (Exception ex)
    {
        ConsoleStyles.PrintError($"{city.Name}: {ex.Message}");
    }
}

ConsoleStyles.PrintFooter();
ConsoleStyles.PrintAttribution("Open-Meteo");