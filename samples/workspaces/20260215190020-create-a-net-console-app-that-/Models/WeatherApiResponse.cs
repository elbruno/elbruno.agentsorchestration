using System.Text.Json.Serialization;

namespace WeatherApp.Models;

/// <summary>
/// Response model for Open-Meteo API (free, no API key required).
/// </summary>
public sealed record WeatherApiResponse(
    [property: JsonPropertyName("current")] CurrentWeather? Current);

/// <summary>
/// Current weather data from Open-Meteo API.
/// </summary>
public sealed record CurrentWeather(
    [property: JsonPropertyName("temperature_2m")] double Temperature,
    [property: JsonPropertyName("weather_code")] int WeatherCode);

/// <summary>
/// City location data for weather lookup.
/// </summary>
public sealed record CityLocation(
    string Name,
    double Latitude,
    double Longitude);
