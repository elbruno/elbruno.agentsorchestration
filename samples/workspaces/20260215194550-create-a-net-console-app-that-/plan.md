# Weather Console App - Implementation Plan

## Overview
.NET console app that displays current weather for Toronto, Tokyo, and Madrid using the **Open-Meteo API** (free, no API key required).

## API Details
- **Service**: Open-Meteo (https://open-meteo.com)
- **Endpoint**: `https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current=temperature_2m`
- **No authentication required**

## City Coordinates
| City    | Latitude | Longitude |
|---------|----------|-----------|
| Toronto | 43.65    | -79.38    |
| Tokyo   | 35.68    | 139.69    |
| Madrid  | 40.42    | -3.70     |

## Emoji Logic
- ☀️ Warm: temperature > 25°C
- 🌤️ Mild: temperature 10-25°C
- ❄️ Cold: temperature < 10°C

---

## Phase 1: Project Setup
- Task: Create WeatherApp.csproj with net10.0 target | Agent: Coder | File: WeatherApp.csproj

## Phase 2: Weather Service
- Task: Create WeatherService class that calls Open-Meteo API using HttpClient, parses JSON response | Agent: Coder | File: WeatherService.cs
  - Dependencies: Phase 1

## Phase 3: Main Program
- Task: Create Program.cs that fetches weather for all three cities and displays formatted output with emoji | Agent: Coder | File: Program.cs
  - Dependencies: Phase 1, Phase 2

## Phase 4: Validation
- Task: Build and validate generated project | Agent: Orchestrator | File: build-output.log
  - Dependencies: Phase 1, Phase 2, Phase 3
