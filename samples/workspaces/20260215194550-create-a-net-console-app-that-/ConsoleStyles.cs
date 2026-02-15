namespace WeatherApp;

/// <summary>
/// Console output styling utilities for the weather display.
/// Provides colored output, emoji selection, and consistent formatting.
/// </summary>
public static class ConsoleStyles
{
    // Temperature thresholds for emoji selection
    private const double WarmThreshold = 25.0;
    private const double ColdThreshold = 10.0;

    /// <summary>
    /// Gets the appropriate weather emoji based on temperature.
    /// ☀️ for warm (>25°C), 🌤️ for mild (10-25°C), ❄️ for cold (<10°C)
    /// </summary>
    public static string GetWeatherEmoji(double temperatureCelsius)
    {
        return temperatureCelsius switch
        {
            > WarmThreshold => "☀️",
            < ColdThreshold => "❄️",
            _ => "🌤️"
        };
    }

    /// <summary>
    /// Prints the application header with styling.
    /// </summary>
    public static void PrintHeader()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════╗");
        Console.WriteLine("║       🌍 World Weather Report        ║");
        Console.WriteLine("╚══════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
    }

    /// <summary>
    /// Prints a city's weather with formatted, colored output.
    /// </summary>
    public static void PrintCityWeather(string cityName, double temperatureCelsius)
    {
        var emoji = GetWeatherEmoji(temperatureCelsius);
        var tempColor = GetTemperatureColor(temperatureCelsius);

        Console.Write("  ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"{cityName,-10}");
        
        Console.ForegroundColor = tempColor;
        Console.Write($"{temperatureCelsius,6:F1}°C ");
        
        Console.ResetColor();
        Console.WriteLine(emoji);
    }

    /// <summary>
    /// Prints a separator line.
    /// </summary>
    public static void PrintSeparator()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  ─────────────────────────────");
        Console.ResetColor();
    }

    /// <summary>
    /// Prints an error message in red.
    /// </summary>
    public static void PrintError(string cityName, string message)
    {
        Console.Write("  ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"{cityName,-10}");
        
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {message}");
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
        Console.ResetColor();
        Console.WriteLine();
    }

    /// <summary>
    /// Gets console color based on temperature range for visual clarity.
    /// </summary>
    private static ConsoleColor GetTemperatureColor(double temperatureCelsius)
    {
        return temperatureCelsius switch
        {
            > WarmThreshold => ConsoleColor.Red,
            < ColdThreshold => ConsoleColor.Blue,
            _ => ConsoleColor.Yellow
        };
    }
}
