Console.WriteLine("╔════════════════════════════════════════╗");
Console.WriteLine("║       Current Weather Report          ║");
Console.WriteLine("╚════════════════════════════════════════╝");
Console.WriteLine();

var cities = new[] { "London", "Tokyo", "New York" };
var random = new Random();

foreach (var city in cities)
{
    var temperature = random.Next(10, 31);
    var emoji = temperature switch
    {
        < 15 => "❄️",
        >= 15 and < 25 => "🌤️",
        _ => "☀️"
    };

    Console.WriteLine($"{emoji} {city,-15} {temperature}°C");
}

Console.WriteLine();
Console.WriteLine("Weather data refreshed successfully!");
