namespace WeatherApp.Models;

/// <summary>
/// Represents weather information for a city.
/// </summary>
public sealed record WeatherData(
    string CityName,
    double TemperatureCelsius,
    string Description)
{
    /// <summary>
    /// Gets an emoji representing the temperature category.
    /// </summary>
    public string WeatherEmoji => TemperatureCelsius switch
    {
        >= 25 => "☀️",  // Warm
        >= 10 => "🌤️", // Mild
        _ => "❄️"       // Cold
    };

    /// <summary>
    /// Returns a formatted display string for the weather.
    /// </summary>
    public override string ToString() =>
        $"{CityName}: {TemperatureCelsius:F1}°C {WeatherEmoji}";
}
