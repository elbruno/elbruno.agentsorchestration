# Research Summary

Researched free weather APIs for .NET applications.

## Key Findings

1. **Open-Meteo**: Free weather API for non-commercial use. No API key required.
2. **OpenWeatherMap**: Free tier available, requires API key.
3. **WeatherAPI**: Free tier available, requires API key.

## Recommended Service: Open-Meteo
- **URL**: https://api.open-meteo.com/v1/forecast
- **Pros**: No API key, simple JSON response, extensive documentation.
- **Cons**: Rate limited for high volume (not an issue for this app).

## Implementation Details
Use `HttpClient` to fetch JSON data. Deserialization with `System.Text.Json`.
