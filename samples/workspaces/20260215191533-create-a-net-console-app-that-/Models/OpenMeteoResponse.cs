using System.Text.Json.Serialization;

namespace WeatherApp.Models;

/// <summary>
/// Response model for Open-Meteo API (free, no API key required).
/// </summary>
public sealed record OpenMeteoResponse(
    [property: JsonPropertyName("current_weather")] CurrentWeather? CurrentWeather);

/// <summary>
/// Current weather data from Open-Meteo API.
/// </summary>
public sealed record CurrentWeather(
    [property: JsonPropertyName("temperature")] double Temperature,
    [property: JsonPropertyName("weathercode")] int WeatherCode,
    [property: JsonPropertyName("windspeed")] double WindSpeed);
