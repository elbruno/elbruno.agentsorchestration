using System.Text.Json.Serialization;

// Weather models
public sealed record WeatherInfo(string City, int Temperature, string Emoji);

public sealed record OpenMeteoResponse(
    [property: JsonPropertyName("current_weather")] CurrentWeather CurrentWeather
);

public sealed record CurrentWeather(
    [property: JsonPropertyName("temperature")] double Temperature,
    [property: JsonPropertyName("windspeed")] double Windspeed,
    [property: JsonPropertyName("weathercode")] int WeatherCode
);
