using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

Console.WriteLine("╔════════════════════════════════════════╗");
Console.WriteLine("║   Live Weather Report (Open-Meteo)    ║");
Console.WriteLine("╚════════════════════════════════════════╝");
Console.WriteLine();

using var client = new HttpClient();
client.BaseAddress = new Uri("https://api.open-meteo.com/v1/");

// Coordinates for: Toronto, Tokyo, Madrid
var cities = new Dictionary<string, (double Lat, double Lon)>
{
    { "Toronto", (43.6532, -79.3832) },
    { "Tokyo", (35.6895, 139.6917) },
    { "Madrid", (40.4168, -3.7038) }
};

Console.WriteLine($"{ "City",-15} { "Temp",-10} { "Condition"}");
Console.WriteLine(new string('-', 40));

foreach (var city in cities)
{
    try
    {
        var url = $"forecast?latitude={city.Value.Lat}&longitude={city.Value.Lon}&current_weather=true";
        var response = await client.GetFromJsonAsync<OpenMeteoResponse>(url);

        if (response?.CurrentWeather is not null)
        {
            var temp = response.CurrentWeather.Temperature;
            var emoji = temp switch
            {
                < 10 => "❄️",
                >= 10 and < 25 => "🌤️",
                _ => "☀️"
            };

            Console.WriteLine($"{emoji} {city.Key,-15} {temp + "°C",-10} {GetWeatherDesc(response.CurrentWeather.WeatherCode)}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ {city.Key}: Error fetching data ({ex.Message})");
    }
}

static string GetWeatherDesc(int code) => code switch
{
    0 => "Clear sky",
    1 or 2 or 3 => "Mainly clear, partly cloudy, and overcast",
    45 or 48 => "Fog",
    _ => "Rain/Cloudy"
};
