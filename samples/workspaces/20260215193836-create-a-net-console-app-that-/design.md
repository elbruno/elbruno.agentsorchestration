# Weather App Console Output Design Spec

## Sample Console Output Mockup

```
🌍 Weather Report
═══════════════════════════════
Toronto     22°C  ☀️
Tokyo       18°C  🌤️
Madrid       8°C  ❄️
═══════════════════════════════
```

## Format String Template

### C# Format String
```csharp
// Header
Console.WriteLine("🌍 Weather Report");
Console.WriteLine("═══════════════════════════════");

// Each city line format:
// {CityName,-12}{Temperature,3}°C  {Emoji}
Console.WriteLine($"{cityName,-12}{temperature,3}°C  {emoji}");

// Footer
Console.WriteLine("═══════════════════════════════");
```

### Emoji Selection Logic
```csharp
string GetWeatherEmoji(int tempCelsius)
{
    if (tempCelsius > 20) return "☀️";   // Warm
    if (tempCelsius >= 10) return "🌤️";  // Mild (10-20°C)
    return "❄️";                          // Cold (<10°C)
}
```

## Special Considerations for Emoji Display

1. **Console Encoding**: Set UTF-8 output encoding at app startup:
   ```csharp
   Console.OutputEncoding = System.Text.Encoding.UTF8;
   ```

2. **Windows Terminal**: Modern Windows Terminal displays emojis correctly. Legacy `cmd.exe` may show placeholder characters.

3. **Column Alignment**: Emojis may have variable display width. The format uses fixed padding before the emoji to maintain alignment.

4. **Font Support**: Ensure the console font supports Unicode emoji glyphs (e.g., Cascadia Code, Segoe UI Emoji).
