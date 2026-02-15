Now I'll create the data models for the weather app. Based on the plan, I need models for the Open-Meteo API response and the weather display data.Now I'll create the data models for the weather app. Based on the plan, I need models for the Open-Meteo API response and the weather display data.Created `Models.cs` with four data models:

1. **CityInfo** - Holds city name and coordinates for API requests
2. **WeatherData** - Display model with city, temperature, and emoji (includes static factory method)
3. **OpenMeteoResponse** - Maps the Open-Meteo API JSON response
4. **CurrentWeather** - Nested current weather data from API

The emoji logic follows the plan: ☀️ (>25°C), 🌤️ (10-25°C), ❄️ (<10°C).Created `Models.cs` with four data models:

1. **CityInfo** - Holds city name and coordinates for API requests
2. **WeatherData** - Display model with city, temperature, and emoji (includes static factory method)
3. **OpenMeteoResponse** - Maps the Open-Meteo API JSON response
4. **CurrentWeather** - Nested current weather data from API

The emoji logic follows the plan: ☀️ (>25°C), 🌤️ (10-25°C), ❄️ (<10°C).