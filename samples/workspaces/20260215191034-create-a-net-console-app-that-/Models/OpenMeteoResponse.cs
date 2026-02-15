using System.Text.Json.Serialization;

namespace WeatherApp.Models;

/// <summary>
/// Response model for Open-Meteo API (free, no API key required).
/// API: https://open-meteo.com/
/// </summary>
public sealed record OpenMeteoResponse(
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude,
    [property: JsonPropertyName("current")] CurrentWeather? Current);

/// <summary>
/// Current weather data from Open-Meteo API.
/// </summary>
public sealed record CurrentWeather(
    [property: JsonPropertyName("temperature_2m")] double Temperature,
    [property: JsonPropertyName("weather_code")] int WeatherCode);
