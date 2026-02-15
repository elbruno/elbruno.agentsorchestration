# .NET Console App Weather Display - Implementation Plan

## Overview
Create a .NET console application that displays current weather for Toronto, Tokyo, and Madrid with emoji indicators.

## API Selection: Open-Meteo

### Why Open-Meteo?
- **Completely free** - No API key required
- **No rate limits** for reasonable usage
- **Simple REST API** - Easy JSON response
- **Reliable** - Well-maintained open-source weather API
- **Current weather** - Supports real-time temperature data

### API Endpoint Format
```
https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current=temperature_2m
```

## City Coordinates & API URLs

### Toronto, Canada
- **Latitude:** 43.65
- **Longitude:** -79.38
- **API URL:**
```
https://api.open-meteo.com/v1/forecast?latitude=43.65&longitude=-79.38&current=temperature_2m
```

### Tokyo, Japan
- **Latitude:** 35.68
- **Longitude:** 139.69
- **API URL:**
```
https://api.open-meteo.com/v1/forecast?latitude=35.68&longitude=139.69&current=temperature_2m
```

### Madrid, Spain
- **Latitude:** 40.42
- **Longitude:** -3.70
- **API URL:**
```
https://api.open-meteo.com/v1/forecast?latitude=40.42&longitude=-3.70&current=temperature_2m
```

## JSON Response Structure
```json
{
  "latitude": 43.646603,
  "longitude": -79.38272,
  "current_units": {
    "temperature_2m": "°C"
  },
  "current": {
    "time": "2026-02-15T19:30",
    "temperature_2m": 0.4
  }
}
```

### Parsing the Temperature
1. Deserialize JSON response
2. Navigate to `current.temperature_2m` property
3. Value is a decimal number in Celsius

## Temperature Thresholds & Emoji Selection

| Condition | Temperature Range | Emoji |
|-----------|-------------------|-------|
| Warm      | > 20°C            | ☀️    |
| Mild      | 10°C to 20°C      | 🌤️    |
| Cold      | < 10°C            | ❄️    |

### Emoji Selection Logic (C#)
```csharp
string GetWeatherEmoji(double temperature)
{
    if (temperature > 20) return "☀️";   // Warm
    if (temperature >= 10) return "🌤️";  // Mild
    return "❄️";                          // Cold
}
```

## Implementation Steps

### 1. Create Project Structure
- Create new .NET console application
- Target .NET 8.0 or later

### 2. Define Data Models
```csharp
public class WeatherResponse
{
    public CurrentWeather Current { get; set; }
}

public class CurrentWeather
{
    [JsonPropertyName("temperature_2m")]
    public double Temperature { get; set; }
}
```

### 3. Create City Configuration
```csharp
public record City(string Name, double Latitude, double Longitude);

var cities = new List<City>
{
    new("Toronto", 43.65, -79.38),
    new("Tokyo", 35.68, 139.69),
    new("Madrid", 40.42, -3.70)
};
```

### 4. Implement HTTP Client Logic
- Use `HttpClient` to fetch weather data
- Use `System.Text.Json` for JSON deserialization
- Handle potential HTTP errors gracefully

### 5. Display Output Format
```
Weather Report
==============
Toronto: 5.2°C ❄️
Tokyo: 15.8°C 🌤️
Madrid: 22.1°C ☀️
```

## Dependencies
- `System.Net.Http` (built-in)
- `System.Text.Json` (built-in)

## Error Handling
- Wrap HTTP calls in try-catch
- Display user-friendly error message if API fails
- Consider timeout handling (default 30 seconds)

## Testing Considerations
- API returns temperatures in Celsius by default
- Test with various temperature ranges
- Verify emoji displays correctly in console (UTF-8 encoding)
