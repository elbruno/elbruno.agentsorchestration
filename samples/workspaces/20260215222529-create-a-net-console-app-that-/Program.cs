using System.Net.Http.Json;
using WeatherApp;
using WeatherApp.Models;

// Display header
ConsoleStyles.PrintHeader();
ConsoleStyles.PrintLoading("Fetching weather data");

using var httpClient = new HttpClient();
var cities = CityCoordinates.DefaultCities;

foreach (var city in cities)
{
    try
    {
        var url = $"https://api.open-meteo.com/v1/forecast?latitude={city.Latitude}&longitude={city.Longitude}&current=temperature_2m,weather_code";
        var response = await httpClient.GetFromJsonAsync<OpenMeteoResponse>(url);
        
        if (response?.Current != null)
        {
            ConsoleStyles.PrintCityWeather(city.Name, response.Current.Temperature);
        }
        else
        {
            ConsoleStyles.PrintCityError(city.Name, "No data available");
        }
    }
    catch (Exception ex)
    {
        ConsoleStyles.PrintCityError(city.Name, ex.Message);
    }
}

// Display footer with timestamp
ConsoleStyles.PrintFooter();