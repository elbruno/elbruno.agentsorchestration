namespace WeatherApp.Models;

/// <summary>
/// Represents geographic coordinates for a city.
/// Used with Open-Meteo API which requires lat/lon parameters.
/// </summary>
public sealed record CityCoordinates(
    string Name,
    double Latitude,
    double Longitude)
{
    /// <summary>
    /// Predefined coordinates for supported cities.
    /// </summary>
    public static readonly CityCoordinates Toronto = new("Toronto", 43.6532, -79.3832);
    public static readonly CityCoordinates Tokyo = new("Tokyo", 35.6762, 139.6503);
    public static readonly CityCoordinates Madrid = new("Madrid", 40.4168, -3.7038);

    /// <summary>
    /// All cities to display weather for.
    /// </summary>
    public static IReadOnlyList<CityCoordinates> AllCities => [Toronto, Tokyo, Madrid];
}
