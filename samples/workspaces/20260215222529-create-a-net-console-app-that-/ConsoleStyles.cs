namespace WeatherApp;

/// <summary>
/// Console styling utilities for weather display.
/// Design intent: Clean, colorful output with clear visual hierarchy.
/// Uses ANSI colors for temperature ranges and weather emojis for quick recognition.
/// </summary>
public static class ConsoleStyles
{
    // Temperature thresholds (Celsius)
    private const int WarmThreshold = 20;
    private const int ColdThreshold = 10;

    /// <summary>
    /// Gets the appropriate weather emoji based on temperature.
    /// ☀️ = warm (≥20°C), 🌤️ = mild (10-19°C), ❄️ = cold (&lt;10°C)
    /// </summary>
    public static string GetWeatherEmoji(double temperatureCelsius) => temperatureCelsius switch
    {
        >= WarmThreshold => "☀️",
        >= ColdThreshold => "🌤️",
        _ => "❄️"
    };

    /// <summary>
    /// Gets the console color for temperature display.
    /// Red = warm, Yellow = mild, Cyan = cold
    /// </summary>
    public static ConsoleColor GetTemperatureColor(double temperatureCelsius) => temperatureCelsius switch
    {
        >= WarmThreshold => ConsoleColor.Red,
        >= ColdThreshold => ConsoleColor.Yellow,
        _ => ConsoleColor.Cyan
    };

    /// <summary>
    /// Prints the application header with decorative border.
    /// </summary>
    public static void PrintHeader()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║       🌍 World Weather Report 🌍       ║");
        Console.WriteLine("╚════════════════════════════════════════╝");
        Console.WriteLine();
        Console.ResetColor();
    }

    /// <summary>
    /// Prints a single city's weather with styled output.
    /// Format: [emoji] CityName: XX.X°C
    /// </summary>
    public static void PrintCityWeather(string cityName, double temperatureCelsius)
    {
        var emoji = GetWeatherEmoji(temperatureCelsius);
        var color = GetTemperatureColor(temperatureCelsius);

        Console.Write($"  {emoji} ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"{cityName,-12}: ");
        Console.ForegroundColor = color;
        Console.WriteLine($"{temperatureCelsius:F1}°C");
        Console.ResetColor();
    }

    /// <summary>
    /// Prints an error message for a city that failed to load.
    /// </summary>
    public static void PrintCityError(string cityName, string error)
    {
        Console.Write("  ⚠️ ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"{cityName,-12}: ");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(error);
        Console.ResetColor();
    }

    /// <summary>
    /// Prints the footer with timestamp.
    /// </summary>
    public static void PrintFooter()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  Updated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine();
        Console.ResetColor();
    }

    /// <summary>
    /// Prints a loading indicator for async operations.
    /// </summary>
    public static void PrintLoading(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  ⏳ {message}...");
        Console.ResetColor();
    }
}
