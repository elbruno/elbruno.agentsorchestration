using System.Text.Json.Serialization;

namespace WeatherApp;

/// <summary>
/// JSON model for Open-Meteo API response.
/// </summary>
public sealed record WeatherResponse(
    [property: JsonPropertyName("current")] CurrentWeather? Current
);

/// <summary>
/// Current weather data from Open-Meteo API.
/// </summary>
public sealed record CurrentWeather(
    [property: JsonPropertyName("temperature_2m")] double Temperature
);