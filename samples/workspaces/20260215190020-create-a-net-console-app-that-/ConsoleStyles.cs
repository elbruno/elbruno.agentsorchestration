namespace WeatherApp;

/// <summary>
/// Console styling utilities for weather display.
/// Uses ANSI escape codes for colors and box-drawing characters for visual structure.
/// Style: Clean, modern terminal aesthetic with emoji weather indicators.
/// </summary>
public static class ConsoleStyles
{
    // ANSI color codes for temperature ranges
    public const string Reset = "\x1b[0m";
    public const string Bold = "\x1b[1m";
    
    // Temperature colors (warm to cold gradient)
    public const string WarmColor = "\x1b[38;5;208m";    // Orange for warm (≥25°C)
    public const string MildColor = "\x1b[38;5;226m";    // Yellow for mild (10-24°C)
    public const string ColdColor = "\x1b[38;5;81m";     // Cyan for cold (<10°C)
    
    // UI element colors
    public const string HeaderColor = "\x1b[38;5;45m";   // Bright cyan for headers
    public const string CityColor = "\x1b[38;5;255m";    // White for city names
    public const string BorderColor = "\x1b[38;5;240m";  // Gray for borders
    
    // Weather emojis based on temperature
    public const string WarmEmoji = "☀️";   // Sunny/warm (≥25°C)
    public const string MildEmoji = "🌤️";  // Partly cloudy/mild (10-24°C)
    public const string ColdEmoji = "❄️";   // Cold (<10°C)
    
    // Box-drawing characters for clean borders
    public const string TopLeft = "╭";
    public const string TopRight = "╮";
    public const string BottomLeft = "╰";
    public const string BottomRight = "╯";
    public const string Horizontal = "─";
    public const string Vertical = "│";
    
    /// <summary>
    /// Gets the appropriate color code based on temperature.
    /// </summary>
    public static string GetTemperatureColor(double tempCelsius) => tempCelsius switch
    {
        >= 25 => WarmColor,
        >= 10 => MildColor,
        _ => ColdColor
    };
    
    /// <summary>
    /// Gets the appropriate weather emoji based on temperature.
    /// </summary>
    public static string GetWeatherEmoji(double tempCelsius) => tempCelsius switch
    {
        >= 25 => WarmEmoji,
        >= 10 => MildEmoji,
        _ => ColdEmoji
    };
    
    /// <summary>
    /// Prints a styled header box.
    /// </summary>
    public static void PrintHeader(string title)
    {
        int width = 40;
        string line = new string('─', width - 2);
        int padding = (width - 2 - title.Length) / 2;
        
        Console.WriteLine();
        Console.WriteLine($"{BorderColor}{TopLeft}{line}{TopRight}{Reset}");
        Console.WriteLine($"{BorderColor}{Vertical}{Reset}{HeaderColor}{Bold}{title.PadLeft(padding + title.Length).PadRight(width - 2)}{Reset}{BorderColor}{Vertical}{Reset}");
        Console.WriteLine($"{BorderColor}{BottomLeft}{line}{BottomRight}{Reset}");
        Console.WriteLine();
    }
    
    /// <summary>
    /// Prints a styled weather card for a city.
    /// </summary>
    public static void PrintWeatherCard(string city, double tempCelsius)
    {
        string emoji = GetWeatherEmoji(tempCelsius);
        string tempColor = GetTemperatureColor(tempCelsius);
        
        Console.WriteLine($"  {CityColor}{Bold}{city,-12}{Reset} {tempColor}{tempCelsius,5:F1}°C{Reset}  {emoji}");
    }
    
    /// <summary>
    /// Prints a separator line.
    /// </summary>
    public static void PrintSeparator()
    {
        Console.WriteLine($"  {BorderColor}{"".PadRight(30, '·')}{Reset}");
    }
    
    /// <summary>
    /// Prints footer with timestamp.
    /// </summary>
    public static void PrintFooter()
    {
        Console.WriteLine();
        Console.WriteLine($"  {BorderColor}Updated: {DateTime.Now:HH:mm:ss}{Reset}");
        Console.WriteLine();
    }
}
