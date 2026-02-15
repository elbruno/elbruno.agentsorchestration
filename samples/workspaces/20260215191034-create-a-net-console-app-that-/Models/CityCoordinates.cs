namespace WeatherApp.Models;

/// <summary>
/// Represents geographic coordinates for a city.
/// Used for Open-Meteo API calls which require lat/lon.
/// </summary>
public sealed record CityCoordinates(
    string Name,
    double Latitude,
    double Longitude)
{
    /// <summary>
    /// Predefined coordinates for the three target cities.
    /// </summary>
    public static readonly CityCoordinates[] TargetCities =
    [
        new("Toronto", 43.6532, -79.3832),
        new("Tokyo", 35.6762, 139.6503),
        new("Madrid", 40.4168, -3.7038)
    ];
}
