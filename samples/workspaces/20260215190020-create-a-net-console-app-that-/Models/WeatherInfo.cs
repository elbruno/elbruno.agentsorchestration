namespace WeatherApp.Models;

/// <summary>
/// Represents weather information for a city.
/// </summary>
public sealed record WeatherInfo(
    string CityName,
    double TemperatureCelsius,
    string Description)
{
    /// <summary>
    /// Gets the appropriate weather emoji based on temperature.
    /// ☀️ for warm (>= 20°C), 🌤️ for mild (10-19°C), ❄️ for cold (< 10°C).
    /// </summary>
    public string WeatherEmoji => TemperatureCelsius switch
    {
        >= 20 => "☀️",
        >= 10 => "🌤️",
        _ => "❄️"
    };

    /// <summary>
    /// Returns a formatted display string for the weather info.
    /// </summary>
    public override string ToString() =>
        $"{CityName}: {TemperatureCelsius:F1}°C {WeatherEmoji}";
}
