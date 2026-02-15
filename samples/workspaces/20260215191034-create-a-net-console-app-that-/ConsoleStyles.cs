namespace WeatherApp;

/// <summary>
/// Console styling utilities for weather display.
/// Uses ANSI escape codes for cross-platform color support.
/// Design: Clean, readable output with temperature-based color coding.
/// </summary>
public static class ConsoleStyles
{
    // ANSI color codes for temperature ranges
    private const string Reset = "\u001b[0m";
    private const string Bold = "\u001b[1m";
    private const string Dim = "\u001b[2m";
    
    // Temperature colors: warm=red/yellow, mild=cyan, cold=blue
    private const string WarmColor = "\u001b[38;5;208m";    // Orange
    private const string MildColor = "\u001b[38;5;117m";    // Light cyan
    private const string ColdColor = "\u001b[38;5;39m";     // Blue
    private const string CityColor = "\u001b[38;5;255m";    // White
    private const string HeaderColor = "\u001b[38;5;141m";  // Purple
    private const string BorderColor = "\u001b[38;5;240m";  // Gray

    /// <summary>
    /// Prints the application header with decorative border.
    /// </summary>
    public static void PrintHeader()
    {
        Console.WriteLine();
        Console.WriteLine($"{BorderColor}╔══════════════════════════════════════════╗{Reset}");
        Console.WriteLine($"{BorderColor}║{Reset}  {HeaderColor}{Bold}🌍 World Weather Dashboard{Reset}              {BorderColor}║{Reset}");
        Console.WriteLine($"{BorderColor}╚══════════════════════════════════════════╝{Reset}");
        Console.WriteLine();
    }

    /// <summary>
    /// Prints a weather card for a city with temperature-based styling.
    /// </summary>
    public static void PrintWeatherCard(string city, double temperatureCelsius, string description)
    {
        var emoji = GetWeatherEmoji(temperatureCelsius);
        var tempColor = GetTemperatureColor(temperatureCelsius);
        var tempFormatted = $"{temperatureCelsius:F1}°C";

        Console.WriteLine($"{BorderColor}┌──────────────────────────────────────────┐{Reset}");
        Console.WriteLine($"{BorderColor}│{Reset}  {emoji}  {CityColor}{Bold}{city,-20}{Reset} {tempColor}{tempFormatted,8}{Reset}  {BorderColor}│{Reset}");
        Console.WriteLine($"{BorderColor}│{Reset}      {Dim}{description,-32}{Reset}  {BorderColor}│{Reset}");
        Console.WriteLine($"{BorderColor}└──────────────────────────────────────────┘{Reset}");
    }

    /// <summary>
    /// Returns weather emoji based on temperature thresholds.
    /// ☀️ warm (>20°C), 🌤️ mild (10-20°C), ❄️ cold (<10°C)
    /// </summary>
    public static string GetWeatherEmoji(double temperatureCelsius)
    {
        return temperatureCelsius switch
        {
            > 20 => "☀️",   // Warm
            >= 10 => "🌤️",  // Mild
            _ => "❄️"       // Cold
        };
    }

    /// <summary>
    /// Returns ANSI color code based on temperature range.
    /// </summary>
    private static string GetTemperatureColor(double temperatureCelsius)
    {
        return temperatureCelsius switch
        {
            > 20 => WarmColor,
            >= 10 => MildColor,
            _ => ColdColor
        };
    }

    /// <summary>
    /// Prints a loading indicator while fetching data.
    /// </summary>
    public static void PrintLoading(string city)
    {
        Console.WriteLine($"{Dim}  ⏳ Fetching weather for {city}...{Reset}");
    }

    /// <summary>
    /// Prints an error message with consistent styling.
    /// </summary>
    public static void PrintError(string message)
    {
        Console.WriteLine($"\u001b[38;5;196m  ⚠️  Error: {message}{Reset}");
    }

    /// <summary>
    /// Prints footer with timestamp.
    /// </summary>
    public static void PrintFooter()
    {
        Console.WriteLine();
        Console.WriteLine($"{Dim}  Last updated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}{Reset}");
        Console.WriteLine();
    }

    /// <summary>
    /// Clears formatting for terminals that don't support ANSI.
    /// Call this if output appears garbled.
    /// </summary>
    public static void EnableVirtualTerminal()
    {
        // On Windows, enable virtual terminal processing
        if (OperatingSystem.IsWindows())
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }
    }
}
