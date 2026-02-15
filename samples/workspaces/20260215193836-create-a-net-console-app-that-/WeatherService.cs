using System.Net.Http.Json;

namespace WeatherApp;

/// <summary>
/// Service for fetching weather data from Open-Meteo API (free, no API key required).
/// </summary>
public sealed class WeatherService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;

    public WeatherService() : this(new HttpClient(), ownsHttpClient: true) { }

    public WeatherService(HttpClient httpClient, bool ownsHttpClient = false)
    {
        _httpClient = httpClient;
        _ownsHttpClient = ownsHttpClient;
    }

    /// <summary>
    /// Fetches current weather for the given coordinates.
    /// </summary>
    public async Task<WeatherResponse?> GetCurrentWeatherAsync(double latitude, double longitude)
    {
        var url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current=temperature_2m";
        return await _httpClient.GetFromJsonAsync<WeatherResponse>(url);
    }

    /// <summary>
    /// Returns a weather emoji based on temperature.
    /// </summary>
    public static string GetWeatherEmoji(double temperatureCelsius) => temperatureCelsius switch
    {
        > 25 => "☀️",   // Warm
        >= 10 => "🌤️", // Mild
        _ => "❄️"       // Cold
    };

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }
}