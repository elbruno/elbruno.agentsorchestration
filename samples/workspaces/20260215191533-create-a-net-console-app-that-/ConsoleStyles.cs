namespace WeatherApp;

/// <summary>
/// Console styling utilities for weather display.
/// Design intent: Clean, accessible output with clear visual hierarchy using
/// ANSI colors, Unicode box-drawing, and weather emojis for quick scanning.
/// </summary>
public static class ConsoleStyles
{
    // Temperature thresholds (Celsius)
    private const int WarmThreshold = 20;
    private const int ColdThreshold = 10;

    // ANSI color codes for temperature ranges
    private static readonly ConsoleColor WarmColor = ConsoleColor.Yellow;
    private static readonly ConsoleColor MildColor = ConsoleColor.Cyan;
    private static readonly ConsoleColor ColdColor = ConsoleColor.Blue;
    private static readonly ConsoleColor HeaderColor = ConsoleColor.White;
    private static readonly ConsoleColor BorderColor = ConsoleColor.DarkGray;

    /// <summary>
    /// Gets the appropriate weather emoji based on temperature.
    /// ☀️ = warm (≥20°C), 🌤️ = mild (10-19°C), ❄️ = cold (<10°C)
    /// </summary>
    public static string GetWeatherEmoji(double temperatureCelsius)
    {
        return temperatureCelsius switch
        {
            >= WarmThreshold => "☀️",
            >= ColdThreshold => "🌤️",
            _ => "❄️"
        };
    }

    /// <summary>
    /// Gets the console color for a temperature value.
    /// </summary>
    public static ConsoleColor GetTemperatureColor(double temperatureCelsius)
    {
        return temperatureCelsius switch
        {
            >= WarmThreshold => WarmColor,
            >= ColdThreshold => MildColor,
            _ => ColdColor
        };
    }

    /// <summary>
    /// Prints the application header with decorative border.
    /// </summary>
    public static void PrintHeader()
    {
        Console.ForegroundColor = BorderColor;
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.ForegroundColor = HeaderColor;
        Console.WriteLine("║       🌍 World Weather Dashboard       ║");
        Console.ForegroundColor = BorderColor;
        Console.WriteLine("╠════════════════════════════════════════╣");
        Console.ResetColor();
    }

    /// <summary>
    /// Prints a single city's weather in a formatted row.
    /// </summary>
    public static void PrintCityWeather(string cityName, double temperatureCelsius)
    {
        var emoji = GetWeatherEmoji(temperatureCelsius);
        var tempColor = GetTemperatureColor(temperatureCelsius);

        Console.ForegroundColor = BorderColor;
        Console.Write("║  ");

        Console.Write(emoji);
        Console.Write(" ");

        Console.ForegroundColor = HeaderColor;
        Console.Write($"{cityName,-12}");

        Console.ForegroundColor = tempColor;
        Console.Write($"{temperatureCelsius,6:F1}°C");

        Console.ForegroundColor = BorderColor;
        Console.WriteLine("              ║");

        Console.ResetColor();
    }

    /// <summary>
    /// Prints the footer border.
    /// </summary>
    public static void PrintFooter()
    {
        Console.ForegroundColor = BorderColor;
        Console.WriteLine("╚════════════════════════════════════════╝");
        Console.ResetColor();
    }

    /// <summary>
    /// Prints a loading indicator for a city.
    /// </summary>
    public static void PrintLoading(string cityName)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  ⏳ Fetching weather for {cityName}...");
        Console.ResetColor();
    }

    /// <summary>
    /// Prints an error message with visual distinction.
    /// </summary>
    public static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  ⚠️  {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Prints the data source attribution.
    /// </summary>
    public static void PrintAttribution(string source)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"\n  Data provided by {source}");
        Console.ResetColor();
    }
}
