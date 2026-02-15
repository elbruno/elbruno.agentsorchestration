using System.Text;

namespace ElBruno.AgentsOrchestration.Agents;

public sealed class TemplateAgentClient : IAgentClient
{
    public Task<string> RunAsync(AgentRole role, string prompt, string workspacePath, CancellationToken cancellationToken)
    {
        var output = role switch
        {
            AgentRole.Planner => BuildPlanOutput(prompt),
            AgentRole.Coder => BuildCoderOutput(prompt),
            AgentRole.Designer => BuildDesignerOutput(prompt),
            AgentRole.Researcher => BuildResearcherOutput(prompt),
            AgentRole.Fixer => BuildFixerOutput(prompt),
            AgentRole.SecurityExpert => BuildSecurityExpertOutput(prompt),
            AgentRole.TestingExpert => BuildTestingExpertOutput(prompt),
            AgentRole.DocumentationExpert => BuildDocumentationExpertOutput(prompt),
            AgentRole.SoftwareArchitect => BuildSoftwareArchitectOutput(prompt),
            _ => $"Orchestration verified for workspace '{workspacePath}'. All tasks completed successfully."
        };

        return Task.FromResult(output);
    }

    private static string BuildPlanOutput(string prompt)
    {
        var sb = new StringBuilder();
        var promptLower = prompt.ToLowerInvariant();
        var isWebApp = promptLower.Contains("web") || promptLower.Contains("blazor") || promptLower.Contains("api") || promptLower.Contains("landing");
        var isResearch = promptLower.Contains("search") || promptLower.Contains("research") || promptLower.Contains("online");

        sb.AppendLine($"# Implementation Plan");
        sb.AppendLine();
        sb.AppendLine($"Prompt: {prompt}");
        sb.AppendLine();

        var phaseIndex = 1;

        if (isResearch)
        {
            sb.AppendLine($"## Phase {phaseIndex++}: Research");
            sb.AppendLine($"- Task: Research available options | Agent: Researcher | File: research.md");
            sb.AppendLine();
        }

        sb.AppendLine($"## Phase {phaseIndex++}: Project Setup");
        sb.AppendLine($"- Task: Create project file | Agent: Coder | File: project.csproj");
        sb.AppendLine();
        sb.AppendLine($"## Phase {phaseIndex++}: Core Implementation");
        sb.AppendLine($"- Task: Implement main application logic | Agent: Coder | File: Program.cs");
        sb.AppendLine($"- Task: Create data models | Agent: Coder | File: Models.cs");

        if (isWebApp)
        {
            sb.AppendLine();
            sb.AppendLine($"## Phase {phaseIndex++}: Styling & UX");
            sb.AppendLine($"- Task: Create application styles | Agent: Designer | File: styles.css");
        }

        return sb.ToString();
    }

    private static string BuildCoderOutput(string prompt)
    {
        var promptLower = prompt.ToLowerInvariant();

        if (promptLower.Contains("project file") || promptLower.Contains(".csproj"))
        {
            return BuildProjectFile(promptLower);
        }

        if (promptLower.Contains("model"))
        {
            return BuildModelsFile(promptLower);
        }

        return BuildProgramFile(promptLower);
    }

    private static string BuildProjectFile(string promptLower)
    {
        var sb = new StringBuilder();
        var sdk = promptLower.Contains("web") || promptLower.Contains("blazor") || promptLower.Contains("api")
            ? "Microsoft.NET.Sdk.Web"
            : "Microsoft.NET.Sdk";
        sb.AppendLine($"<Project Sdk=\"{sdk}\">");
        sb.AppendLine();
        sb.AppendLine("  <PropertyGroup>");
        sb.AppendLine("    <OutputType>Exe</OutputType>");
        sb.AppendLine("    <TargetFramework>net10.0</TargetFramework>");
        sb.AppendLine("    <Nullable>enable</Nullable>");
        sb.AppendLine("    <ImplicitUsings>enable</ImplicitUsings>");
        sb.AppendLine("  </PropertyGroup>");
        sb.AppendLine();
        sb.AppendLine("</Project>");
        return sb.ToString();
    }

    private static string BuildProgramFile(string promptLower)
    {
        var sb = new StringBuilder();

        if (promptLower.Contains("weather"))
        {
            if (promptLower.Contains("search") || promptLower.Contains("online") || promptLower.Contains("research"))
            {
                sb.AppendLine("using System.Net.Http.Json;");
                sb.AppendLine("using System.Text.Json;");
                sb.AppendLine("using System.Text.Json.Serialization;");
                sb.AppendLine();
                sb.AppendLine("Console.WriteLine(\"╔════════════════════════════════════════╗\");");
                sb.AppendLine("Console.WriteLine(\"║   Live Weather Report (Open-Meteo)    ║\");");
                sb.AppendLine("Console.WriteLine(\"╚════════════════════════════════════════╝\");");
                sb.AppendLine("Console.WriteLine();");
                sb.AppendLine();
                sb.AppendLine("using var client = new HttpClient();");
                sb.AppendLine("client.BaseAddress = new Uri(\"https://api.open-meteo.com/v1/\");");
                sb.AppendLine();
                sb.AppendLine("// Coordinates for: Toronto, Tokyo, Madrid");
                sb.AppendLine("var cities = new Dictionary<string, (double Lat, double Lon)>");
                sb.AppendLine("{");
                sb.AppendLine("    { \"Toronto\", (43.6532, -79.3832) },");
                sb.AppendLine("    { \"Tokyo\", (35.6895, 139.6917) },");
                sb.AppendLine("    { \"Madrid\", (40.4168, -3.7038) }");
                sb.AppendLine("};");
                sb.AppendLine();
                sb.AppendLine("Console.WriteLine($\"{ \"City\",-15} { \"Temp\",-10} { \"Condition\"}\");");
                sb.AppendLine("Console.WriteLine(new string('-', 40));");
                sb.AppendLine();
                sb.AppendLine("foreach (var city in cities)");
                sb.AppendLine("{");
                sb.AppendLine("    try");
                sb.AppendLine("    {");
                sb.AppendLine("        var url = $\"forecast?latitude={city.Value.Lat}&longitude={city.Value.Lon}&current_weather=true\";");
                sb.AppendLine("        var response = await client.GetFromJsonAsync<OpenMeteoResponse>(url);");
                sb.AppendLine();
                sb.AppendLine("        if (response?.CurrentWeather is not null)");
                sb.AppendLine("        {");
                sb.AppendLine("            var temp = response.CurrentWeather.Temperature;");
                sb.AppendLine("            var emoji = temp switch");
                sb.AppendLine("            {");
                sb.AppendLine("                < 10 => \"❄️\",");
                sb.AppendLine("                >= 10 and < 25 => \"🌤️\",");
                sb.AppendLine("                _ => \"☀️\"");
                sb.AppendLine("            };");
                sb.AppendLine();
                sb.AppendLine("            Console.WriteLine($\"{emoji} {city.Key,-15} {temp + \"°C\",-10} {GetWeatherDesc(response.CurrentWeather.WeatherCode)}\");");
                sb.AppendLine("        }");
                sb.AppendLine("    }");
                sb.AppendLine("    catch (Exception ex)");
                sb.AppendLine("    {");
                sb.AppendLine("        Console.WriteLine($\"❌ {city.Key}: Error fetching data ({ex.Message})\");");
                sb.AppendLine("    }");
                sb.AppendLine("}");
                sb.AppendLine();
                sb.AppendLine("static string GetWeatherDesc(int code) => code switch");
                sb.AppendLine("{");
                sb.AppendLine("    0 => \"Clear sky\",");
                sb.AppendLine("    1 or 2 or 3 => \"Mainly clear, partly cloudy, and overcast\",");
                sb.AppendLine("    45 or 48 => \"Fog\",");
                sb.AppendLine("    _ => \"Rain/Cloudy\"");
                sb.AppendLine("};");
            }
            else
            {
                sb.AppendLine("Console.WriteLine(\"╔════════════════════════════════════════╗\");");
                sb.AppendLine("Console.WriteLine(\"║       Current Weather Report          ║\");");
                sb.AppendLine("Console.WriteLine(\"╚════════════════════════════════════════╝\");");
                sb.AppendLine("Console.WriteLine();");
                sb.AppendLine();
                sb.AppendLine("var cities = new[] { \"London\", \"Tokyo\", \"New York\" };");
                sb.AppendLine("var random = new Random();");
                sb.AppendLine();
                sb.AppendLine("foreach (var city in cities)");
                sb.AppendLine("{");
                sb.AppendLine("    var temperature = random.Next(10, 31);");
                sb.AppendLine("    var emoji = temperature switch");
                sb.AppendLine("    {");
                sb.AppendLine("        < 15 => \"❄️\",");
                sb.AppendLine("        >= 15 and < 25 => \"🌤️\",");
                sb.AppendLine("        _ => \"☀️\"");
                sb.AppendLine("    };");
                sb.AppendLine();
                sb.AppendLine("    Console.WriteLine($\"{emoji} {city,-15} {temperature}°C\");");
                sb.AppendLine("}");
                sb.AppendLine();
                sb.AppendLine("Console.WriteLine();");
                sb.AppendLine("Console.WriteLine(\"Weather data refreshed successfully!\");");
            }
        }
        else if (promptLower.Contains("csv") && promptLower.Contains("group"))
        {
            sb.AppendLine("if (args.Length < 2)");
            sb.AppendLine("{");
            sb.AppendLine("    Console.WriteLine(\"Usage: app <csv-file> <column-name>\");");
            sb.AppendLine("    return 1;");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("var filePath = args[0];");
            sb.AppendLine("var columnName = args[1];");
            sb.AppendLine();
            sb.AppendLine("if (!File.Exists(filePath))");
            sb.AppendLine("{");
            sb.AppendLine("    Console.WriteLine($\"File not found: {filePath}\");");
            sb.AppendLine("    return 1;");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("var lines = File.ReadAllLines(filePath);");
            sb.AppendLine("if (lines.Length == 0)");
            sb.AppendLine("{");
            sb.AppendLine("    Console.WriteLine(\"CSV file is empty.\");");
            sb.AppendLine("    return 1;");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("var headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();");
            sb.AppendLine("var columnIndex = Array.FindIndex(headers, h => h.Equals(columnName, StringComparison.OrdinalIgnoreCase));");
            sb.AppendLine();
            sb.AppendLine("if (columnIndex < 0)");
            sb.AppendLine("{");
            sb.AppendLine("    Console.WriteLine($\"Column '{columnName}' not found. Available: {string.Join(\", \", headers)}\");");
            sb.AppendLine("    return 1;");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("var groups = lines");
            sb.AppendLine("    .Skip(1)");
            sb.AppendLine("    .Select(line => line.Split(','))");
            sb.AppendLine("    .Where(parts => parts.Length > columnIndex)");
            sb.AppendLine("    .GroupBy(parts => parts[columnIndex].Trim())");
            sb.AppendLine("    .OrderByDescending(g => g.Count());");
            sb.AppendLine();
            sb.AppendLine("Console.WriteLine($\"\\nGrouped by {columnName}:\");");
            sb.AppendLine("Console.WriteLine(new string('-', 40));");
            sb.AppendLine();
            sb.AppendLine("foreach (var group in groups)");
            sb.AppendLine("{");
            sb.AppendLine("    Console.WriteLine($\"{group.Key,-30} {group.Count()}\");");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("return 0;");
        }
        else if (promptLower.Contains("blazor") && promptLower.Contains("recipe"))
        {
            sb.AppendLine("var builder = WebApplication.CreateBuilder(args);");
            sb.AppendLine("builder.Services.AddRazorComponents().AddInteractiveServerComponents();");
            sb.AppendLine("builder.Services.AddSingleton<RecipeService>();");
            sb.AppendLine();
            sb.AppendLine("var app = builder.Build();");
            sb.AppendLine("app.UseStaticFiles();");
            sb.AppendLine("app.UseAntiforgery();");
            sb.AppendLine("app.MapRazorComponents<App>().AddInteractiveServerRenderMode();");
            sb.AppendLine("app.Run();");
        }
        else if (promptLower.Contains("api") && (promptLower.Contains("to-do") || promptLower.Contains("todo")))
        {
            sb.AppendLine("using System.Collections.Concurrent;");
            sb.AppendLine();
            sb.AppendLine("var builder = WebApplication.CreateSlimBuilder(args);");
            sb.AppendLine("var app = builder.Build();");
            sb.AppendLine();
            sb.AppendLine("var todos = new ConcurrentDictionary<int, TodoItem>();");
            sb.AppendLine("var nextId = 0;");
            sb.AppendLine();
            sb.AppendLine("app.MapGet(\"/todos\", () => todos.Values);");
            sb.AppendLine("app.MapGet(\"/todos/{id}\", (int id) =>");
            sb.AppendLine("    todos.TryGetValue(id, out var todo) ? Results.Ok(todo) : Results.NotFound());");
            sb.AppendLine("app.MapPost(\"/todos\", (TodoItem item) =>");
            sb.AppendLine("{");
            sb.AppendLine("    var id = Interlocked.Increment(ref nextId);");
            sb.AppendLine("    var todo = item with { Id = id };");
            sb.AppendLine("    todos[id] = todo;");
            sb.AppendLine("    return Results.Created($\"/todos/{id}\", todo);");
            sb.AppendLine("});");
            sb.AppendLine("app.MapPut(\"/todos/{id}\", (int id, TodoItem item) =>");
            sb.AppendLine("    todos.ContainsKey(id) ? Results.Ok(todos[id] = item with { Id = id }) : Results.NotFound());");
            sb.AppendLine("app.MapDelete(\"/todos/{id}\", (int id) =>");
            sb.AppendLine("    todos.TryRemove(id, out _) ? Results.NoContent() : Results.NotFound());");
            sb.AppendLine();
            sb.AppendLine("app.Run();");
        }
        else
        {
            sb.AppendLine($"// Application entry point");
            sb.AppendLine($"// Generated for: {promptLower}");
            sb.AppendLine();
            sb.AppendLine("Console.WriteLine(\"Application started.\");");
        }

        return sb.ToString();
    }

    private static string BuildModelsFile(string promptLower)
    {
        var sb = new StringBuilder();

        if (promptLower.Contains("weather"))
        {
            if (promptLower.Contains("search") || promptLower.Contains("online") || promptLower.Contains("research"))
            {
                sb.AppendLine("using System.Text.Json.Serialization;");
                sb.AppendLine();
            }

            sb.AppendLine("// Weather models");
            sb.AppendLine("public sealed record WeatherInfo(string City, int Temperature, string Emoji);");

            if (promptLower.Contains("search") || promptLower.Contains("online") || promptLower.Contains("research"))
            {
                sb.AppendLine();
                sb.AppendLine("public sealed record OpenMeteoResponse(");
                sb.AppendLine("    [property: JsonPropertyName(\"current_weather\")] CurrentWeather CurrentWeather");
                sb.AppendLine(");");
                sb.AppendLine();
                sb.AppendLine("public sealed record CurrentWeather(");
                sb.AppendLine("    [property: JsonPropertyName(\"temperature\")] double Temperature,");
                sb.AppendLine("    [property: JsonPropertyName(\"windspeed\")] double Windspeed,");
                sb.AppendLine("    [property: JsonPropertyName(\"weathercode\")] int WeatherCode");
                sb.AppendLine(");");
            }
        }
        else if (promptLower.Contains("recipe"))
        {
            sb.AppendLine("public sealed record Recipe(int Id, string Name, List<string> Ingredients, string Instructions);");
        }
        else if (promptLower.Contains("todo") || promptLower.Contains("to-do"))
        {
            sb.AppendLine("public sealed record TodoItem(int Id, string Title, bool IsComplete);");
        }
        else
        {
            sb.AppendLine($"// Data models");
            sb.AppendLine("public sealed record AppModel(int Id, string Name);");
        }

        return sb.ToString();
    }

    private static string BuildFixerOutput(string prompt)
    {
        var sb = new StringBuilder();

        if (prompt.Contains(".csproj", StringComparison.OrdinalIgnoreCase) || prompt.Contains("SDK", StringComparison.OrdinalIgnoreCase))
        {
            // .csproj files must be valid XML, so no C# comments
            sb.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
            sb.AppendLine();
            sb.AppendLine("  <PropertyGroup>");
            sb.AppendLine("    <OutputType>Exe</OutputType>");
            sb.AppendLine("    <TargetFramework>net10.0</TargetFramework>");
            sb.AppendLine("    <Nullable>enable</Nullable>");
            sb.AppendLine("    <ImplicitUsings>enable</ImplicitUsings>");
            sb.AppendLine("  </PropertyGroup>");
            sb.AppendLine();
            sb.AppendLine("</Project>");
        }
        else
        {
            sb.AppendLine("// Applied fixes to resolve compilation errors");
            sb.AppendLine("Console.WriteLine(\"Application started.\");");
        }

        return sb.ToString();
    }

    private static string BuildDesignerOutput(string prompt)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"/* Styles generated for: {prompt} */");
        sb.AppendLine();
        sb.AppendLine(":root {");
        sb.AppendLine("    --primary: #0d6efd;");
        sb.AppendLine("    --bg: #f8f9fa;");
        sb.AppendLine("    --text: #212529;");
        sb.AppendLine("    --border: #dee2e6;");
        sb.AppendLine("    --radius: 0.5rem;");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("body {");
        sb.AppendLine("    font-family: 'Segoe UI', system-ui, sans-serif;");
        sb.AppendLine("    background-color: var(--bg);");
        sb.AppendLine("    color: var(--text);");
        sb.AppendLine("    margin: 0;");
        sb.AppendLine("    padding: 2rem;");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine(".container {");
        sb.AppendLine("    max-width: 960px;");
        sb.AppendLine("    margin: 0 auto;");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine(".card {");
        sb.AppendLine("    background: white;");
        sb.AppendLine("    border: 1px solid var(--border);");
        sb.AppendLine("    border-radius: var(--radius);");
        sb.AppendLine("    padding: 1.5rem;");
        sb.AppendLine("    margin-bottom: 1rem;");
        sb.AppendLine("    box-shadow: 0 1px 3px rgba(0,0,0,0.08);");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("h1, h2, h3 {");
        sb.AppendLine("    color: var(--primary);");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("table {");
        sb.AppendLine("    width: 100%;");
        sb.AppendLine("    border-collapse: collapse;");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("th, td {");
        sb.AppendLine("    padding: 0.75rem;");
        sb.AppendLine("    text-align: left;");
        sb.AppendLine("    border-bottom: 1px solid var(--border);");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("th {");
        sb.AppendLine("    background-color: var(--primary);");
        sb.AppendLine("    color: white;");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string BuildResearcherOutput(string prompt)
    {
        var sb = new StringBuilder();
        var promptLower = prompt.ToLowerInvariant();

        // Simulate research response based on common patterns
        sb.AppendLine("# Research Summary");
        sb.AppendLine();

        if (promptLower.Contains("weather"))
        {
            sb.AppendLine("Researched free weather APIs for .NET applications.");
            sb.AppendLine();
            sb.AppendLine("## Key Findings");
            sb.AppendLine();
            sb.AppendLine("1. **Open-Meteo**: Free weather API for non-commercial use. No API key required.");
            sb.AppendLine("2. **OpenWeatherMap**: Free tier available, requires API key.");
            sb.AppendLine("3. **WeatherAPI**: Free tier available, requires API key.");
            sb.AppendLine();
            sb.AppendLine("## Recommended Service: Open-Meteo");
            sb.AppendLine("- **URL**: https://api.open-meteo.com/v1/forecast");
            sb.AppendLine("- **Pros**: No API key, simple JSON response, extensive documentation.");
            sb.AppendLine("- **Cons**: Rate limited for high volume (not an issue for this app).");
            sb.AppendLine();
            sb.AppendLine("## Implementation Details");
            sb.AppendLine("Use `HttpClient` to fetch JSON data. Deserialization with `System.Text.Json`.");
        }
        else if (promptLower.Contains("httpclient") || promptLower.Contains("retry") || promptLower.Contains("polly"))
        {
            sb.AppendLine("Researched best practices for implementing retry policies in .NET HttpClient.");
            sb.AppendLine("Found 3 authoritative sources with implementation patterns.");
            sb.AppendLine();
            sb.AppendLine("## Key Findings");
            sb.AppendLine();
            sb.AppendLine("1. **Polly Library**: Industry-standard resilience library with built-in retry policies");
            sb.AppendLine("2. **IHttpClientFactory**: .NET Core 2.1+ feature with native retry support via Polly integration");
            sb.AppendLine("3. **Exponential Backoff**: Recommended pattern to avoid overwhelming downstream services");
            sb.AppendLine();
            sb.AppendLine("## Sources");
            sb.AppendLine();
            sb.AppendLine("### Microsoft Learn: Making HTTP requests with IHttpClientFactory");
            sb.AppendLine("- **Type**: docs");
            sb.AppendLine("- **URL**: https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory");
            sb.AppendLine("- **Relevance**: Official Microsoft documentation on HttpClient best practices");
            sb.AppendLine("- **Key Excerpt**: \"IHttpClientFactory can be configured to use Polly-based policies for resilience and transient fault handling.\"");
            sb.AppendLine();
            sb.AppendLine("### Polly Documentation: Retry Policy");
            sb.AppendLine("- **Type**: library-docs");
            sb.AppendLine("- **URL**: https://github.com/App-vNext/Polly#retry");
            sb.AppendLine("- **Relevance**: Official Polly documentation with code examples");
            sb.AppendLine("- **Key Excerpt**: \"Retry can be configured for exponential backoff with jitter to avoid retry storms.\"");
            sb.AppendLine();
            sb.AppendLine("### .NET Blog: Resilient HTTP Clients");
            sb.AppendLine("- **Type**: web");
            sb.AppendLine("- **URL**: https://devblogs.microsoft.com/dotnet/resilient-http-clients/");
            sb.AppendLine("- **Relevance**: Best practices from .NET team");
            sb.AppendLine("- **Key Excerpt**: \"Combine IHttpClientFactory with Polly for production-grade resilience.\"");
            sb.AppendLine();
            sb.AppendLine("## Recommendations");
            sb.AppendLine();
            sb.AppendLine("1. Use `IHttpClientFactory` with Polly integration via `AddPolicyHandler()`");
            sb.AppendLine("2. Implement exponential backoff: `WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))`");
            sb.AppendLine("3. Add jitter to prevent retry storms in distributed systems");
            sb.AppendLine("4. Log retry attempts for observability");
        }
        else if (promptLower.Contains("error") || promptLower.Contains("cs0246") || promptLower.Contains("build"))
        {
            sb.AppendLine("Researched common build error solutions and namespace resolution issues.");
            sb.AppendLine();
            sb.AppendLine("## Key Findings");
            sb.AppendLine();
            sb.AppendLine("1. **Missing Using Statements**: Most CS0246 errors are caused by missing `using` directives");
            sb.AppendLine("2. **SDK Version Mismatch**: Verify project targets correct .NET SDK version");
            sb.AppendLine("3. **Package References**: Ensure required NuGet packages are referenced in .csproj");
            sb.AppendLine();
            sb.AppendLine("## Sources");
            sb.AppendLine();
            sb.AppendLine("### Microsoft Learn: CS0246 Compiler Error");
            sb.AppendLine("- **Type**: docs");
            sb.AppendLine("- **URL**: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs0246");
            sb.AppendLine("- **Key Excerpt**: \"Add the required using directive or fully qualify the type name.\"");
            sb.AppendLine();
            sb.AppendLine("## Recommendations");
            sb.AppendLine();
            sb.AppendLine("1. Add missing `using` statements at top of file");
            sb.AppendLine("2. Verify .csproj references required packages");
            sb.AppendLine("3. Check SDK version in global.json or .csproj");
        }
        else if (promptLower.Contains("css") || promptLower.Contains("grid") || promptLower.Contains("layout"))
        {
            sb.AppendLine("Researched modern CSS Grid layout patterns for responsive dashboards.");
            sb.AppendLine();
            sb.AppendLine("## Key Findings");
            sb.AppendLine();
            sb.AppendLine("1. **CSS Grid**: Native browser support for 2D layouts without frameworks");
            sb.AppendLine("2. **Responsive Patterns**: `grid-template-columns: repeat(auto-fit, minmax(300px, 1fr))`");
            sb.AppendLine("3. **Dashboard Layouts**: Combine Grid for structure with Flexbox for components");
            sb.AppendLine();
            sb.AppendLine("## Sources");
            sb.AppendLine();
            sb.AppendLine("### MDN Web Docs: CSS Grid Layout");
            sb.AppendLine("- **Type**: docs");
            sb.AppendLine("- **URL**: https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_grid_layout");
            sb.AppendLine("- **Key Excerpt**: \"CSS Grid is perfect for dashboard layouts with mixed column/row spans.\"");
            sb.AppendLine();
            sb.AppendLine("### CSS-Tricks: Complete Guide to Grid");
            sb.AppendLine("- **Type**: web");
            sb.AppendLine("- **URL**: https://css-tricks.com/snippets/css/complete-guide-grid/");
            sb.AppendLine("- **Key Excerpt**: \"Use minmax() and auto-fit for truly responsive grids.\"");
            sb.AppendLine();
            sb.AppendLine("## Recommendations");
            sb.AppendLine();
            sb.AppendLine("1. Use `display: grid` with `grid-template-areas` for semantic dashboard regions");
            sb.AppendLine("2. Implement responsive breakpoints with `@media` queries");
            sb.AppendLine("3. Combine with `gap` property for consistent spacing");
        }
        else
        {
            // Generic research response
            sb.AppendLine($"Research completed for query. Multiple sources analyzed.");
            sb.AppendLine();
            sb.AppendLine("## Key Findings");
            sb.AppendLine();
            sb.AppendLine("1. Authoritative documentation sources located");
            sb.AppendLine("2. Best practices identified from official channels");
            sb.AppendLine("3. Code examples and implementation patterns found");
            sb.AppendLine();
            sb.AppendLine("## Sources");
            sb.AppendLine();
            sb.AppendLine("### Microsoft Learn Documentation");
            sb.AppendLine("- **Type**: docs");
            sb.AppendLine("- **URL**: https://learn.microsoft.com/");
            sb.AppendLine("- **Relevance**: Official Microsoft documentation");
            sb.AppendLine();
            sb.AppendLine("## Recommendations");
            sb.AppendLine();
            sb.AppendLine("Based on research findings, follow official documentation and established best practices.");
        }

        return sb.ToString();
    }

    private static string BuildSecurityExpertOutput(string prompt)
    {
        var sb = new StringBuilder();
        var promptLower = prompt.ToLowerInvariant();

        sb.AppendLine("# Security Review Report");
        sb.AppendLine();
        sb.AppendLine("## Summary");
        sb.AppendLine("Overall Security Posture: **Good** with some recommendations for improvement.");
        sb.AppendLine();
        sb.AppendLine("## Findings");
        sb.AppendLine();
        sb.AppendLine("### ✅ Positive Observations");
        sb.AppendLine("- No hardcoded secrets detected in source files");
        sb.AppendLine("- Proper use of parameterized queries (Entity Framework)");
        sb.AppendLine("- Model validation attributes present on DTOs");

        if (promptLower.Contains("api") || promptLower.Contains("web"))
        {
            sb.AppendLine("- HTTPS enforcement detected in configuration");
            sb.AppendLine();
            sb.AppendLine("### 🟡 Warnings");
            sb.AppendLine("- **Input Validation**: Consider adding rate limiting on API endpoints");
            sb.AppendLine("  - Location: `Program.cs`");
            sb.AppendLine("  - Recommendation: Add `AddRateLimiter()` middleware");
            sb.AppendLine();
            sb.AppendLine("- **Authentication**: If authentication is added, ensure JWT tokens have appropriate expiration");
            sb.AppendLine("  - Recommendation: Use sliding expiration with 15-minute refresh tokens");
            sb.AppendLine();
            sb.AppendLine("- **CORS Configuration**: If CORS is needed, avoid using `AllowAnyOrigin()` in production");
            sb.AppendLine("  - Recommendation: Specify explicit allowed origins");
        }
        else
        {
            sb.AppendLine();
            sb.AppendLine("### 🟡 Warnings");
            sb.AppendLine("- **Input Validation**: Consider validating user input if processing external data");
            sb.AppendLine("- **Error Handling**: Ensure sensitive information is not leaked in exception messages");
        }

        sb.AppendLine();
        sb.AppendLine("### 🔴 Critical Issues");
        sb.AppendLine("None detected.");
        sb.AppendLine();
        sb.AppendLine("## Recommendations");
        sb.AppendLine("1. Add rate limiting for API endpoints (Medium Priority)");
        sb.AppendLine("2. Implement security headers (CSP, X-Frame-Options) for web apps (Medium Priority)");
        sb.AppendLine("3. Consider adding logging for security events (Low Priority)");
        sb.AppendLine("4. Review dependency versions for known CVEs (Low Priority)");
        sb.AppendLine();
        sb.AppendLine("## OWASP Top 10 Coverage");
        sb.AppendLine("- ✅ A01:2021 – Broken Access Control: N/A (no authentication)");
        sb.AppendLine("- ✅ A02:2021 – Cryptographic Failures: No sensitive data stored");
        sb.AppendLine("- ✅ A03:2021 – Injection: Parameterized queries used");
        sb.AppendLine("- ⚠️ A04:2021 – Insecure Design: Consider adding rate limiting");
        sb.AppendLine("- ✅ A05:2021 – Security Misconfiguration: HTTPS enforced");

        return sb.ToString();
    }

    private static string BuildTestingExpertOutput(string prompt)
    {
        var sb = new StringBuilder();
        var promptLower = prompt.ToLowerInvariant();

        sb.AppendLine("# Test Strategy");
        sb.AppendLine();
        sb.AppendLine("## Coverage Summary");
        sb.AppendLine("- **Unit Tests**: 1 test file generated");
        sb.AppendLine("- **Test Coverage Goal**: 70-80% (focus on critical paths)");
        sb.AppendLine();
        sb.AppendLine("## Test Files Generated");
        sb.AppendLine();
        sb.AppendLine("### AppTests.cs");
        sb.AppendLine("```csharp");
        sb.AppendLine("using Xunit;");
        sb.AppendLine();

        if (promptLower.Contains("weather"))
        {
            sb.AppendLine("public class WeatherTests");
            sb.AppendLine("{");
            sb.AppendLine("    [Fact]");
            sb.AppendLine("    public void WeatherInfo_ValidData_CreatesCorrectly()");
            sb.AppendLine("    {");
            sb.AppendLine("        // Arrange");
            sb.AppendLine("        var city = \"London\";");
            sb.AppendLine("        var temperature = 20;");
            sb.AppendLine("        var emoji = \"🌤️\";");
            sb.AppendLine();
            sb.AppendLine("        // Act");
            sb.AppendLine("        var weather = new WeatherInfo(city, temperature, emoji);");
            sb.AppendLine();
            sb.AppendLine("        // Assert");
            sb.AppendLine("        Assert.Equal(city, weather.City);");
            sb.AppendLine("        Assert.Equal(temperature, weather.Temperature);");
            sb.AppendLine("        Assert.Equal(emoji, weather.Emoji);");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    [Theory]");
            sb.AppendLine("    [InlineData(10, \"❄️\")]");
            sb.AppendLine("    [InlineData(20, \"🌤️\")]");
            sb.AppendLine("    [InlineData(30, \"☀️\")]");
            sb.AppendLine("    public void GetWeatherEmoji_TemperatureRange_ReturnsCorrectEmoji(int temp, string expected)");
            sb.AppendLine("    {");
            sb.AppendLine("        // Arrange & Act");
            sb.AppendLine("        var emoji = temp switch");
            sb.AppendLine("        {");
            sb.AppendLine("            < 15 => \"❄️\",");
            sb.AppendLine("            >= 15 and < 25 => \"🌤️\",");
            sb.AppendLine("            _ => \"☀️\"");
            sb.AppendLine("        };");
            sb.AppendLine();
            sb.AppendLine("        // Assert");
            sb.AppendLine("        Assert.Equal(expected, emoji);");
            sb.AppendLine("    }");
            sb.AppendLine("}");
        }
        else if (promptLower.Contains("todo") || promptLower.Contains("api"))
        {
            sb.AppendLine("public class TodoApiTests");
            sb.AppendLine("{");
            sb.AppendLine("    [Fact]");
            sb.AppendLine("    public void TodoItem_ValidData_CreatesCorrectly()");
            sb.AppendLine("    {");
            sb.AppendLine("        // Arrange");
            sb.AppendLine("        var id = 1;");
            sb.AppendLine("        var title = \"Test Todo\";");
            sb.AppendLine("        var isComplete = false;");
            sb.AppendLine();
            sb.AppendLine("        // Act");
            sb.AppendLine("        var todo = new TodoItem(id, title, isComplete);");
            sb.AppendLine();
            sb.AppendLine("        // Assert");
            sb.AppendLine("        Assert.Equal(id, todo.Id);");
            sb.AppendLine("        Assert.Equal(title, todo.Title);");
            sb.AppendLine("        Assert.False(todo.IsComplete);");
            sb.AppendLine("    }");
            sb.AppendLine("}");
        }
        else
        {
            sb.AppendLine("public class AppTests");
            sb.AppendLine("{");
            sb.AppendLine("    [Fact]");
            sb.AppendLine("    public void ApplicationLogic_ValidInput_ReturnsExpectedResult()");
            sb.AppendLine("    {");
            sb.AppendLine("        // Arrange");
            sb.AppendLine("        var input = \"test\";");
            sb.AppendLine();
            sb.AppendLine("        // Act");
            sb.AppendLine("        var result = ProcessInput(input);");
            sb.AppendLine();
            sb.AppendLine("        // Assert");
            sb.AppendLine("        Assert.NotNull(result);");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    private static string ProcessInput(string input) => input;");
            sb.AppendLine("}");
        }

        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("## Running Tests");
        sb.AppendLine("```bash");
        sb.AppendLine("dotnet test");
        sb.AppendLine("dotnet test --configuration Release");
        sb.AppendLine("dotnet test --collect:\"XPlat Code Coverage\"");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("## Recommendations");
        sb.AppendLine("1. Add integration tests for API endpoints using WebApplicationFactory");
        sb.AppendLine("2. Consider mutation testing with Stryker.NET");
        sb.AppendLine("3. Set up code coverage reporting in CI/CD pipeline");
        sb.AppendLine("4. Add performance tests for critical operations");

        return sb.ToString();
    }

    private static string BuildDocumentationExpertOutput(string prompt)
    {
        var sb = new StringBuilder();
        var promptLower = prompt.ToLowerInvariant();

        sb.AppendLine("# Documentation Generated");
        sb.AppendLine();
        sb.AppendLine("## Files Created");
        sb.AppendLine("- ✅ `README.md` — Main project documentation");
        sb.AppendLine("- ✅ `docs/architecture.md` — System architecture");

        if (promptLower.Contains("api") || promptLower.Contains("web"))
        {
            sb.AppendLine("- ✅ `docs/api.md` — API reference");
        }

        sb.AppendLine();
        sb.AppendLine("## README.md Preview");
        sb.AppendLine("```markdown");
        sb.AppendLine($"# Application");
        sb.AppendLine();
        sb.AppendLine("A .NET 10 application demonstrating modern development practices.");
        sb.AppendLine();
        sb.AppendLine("## Features");
        sb.AppendLine();

        if (promptLower.Contains("weather"))
        {
            sb.AppendLine("- ✅ Weather data display for multiple cities");
            sb.AppendLine("- ✅ Temperature-based emoji indicators");
            sb.AppendLine("- ✅ Console-based visualization");
        }
        else if (promptLower.Contains("todo") || promptLower.Contains("api"))
        {
            sb.AppendLine("- ✅ RESTful API with CRUD operations");
            sb.AppendLine("- ✅ In-memory data storage");
            sb.AppendLine("- ✅ Minimal API endpoints");
        }
        else
        {
            sb.AppendLine("- ✅ Modern .NET 10 architecture");
            sb.AppendLine("- ✅ Clean, maintainable code");
        }

        sb.AppendLine();
        sb.AppendLine("## Prerequisites");
        sb.AppendLine();
        sb.AppendLine("- .NET 10.0 SDK or later");
        sb.AppendLine();
        sb.AppendLine("## Installation");
        sb.AppendLine();
        sb.AppendLine("```bash");
        sb.AppendLine("git clone <repository-url>");
        sb.AppendLine("cd <project-directory>");
        sb.AppendLine("dotnet restore");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("## Usage");
        sb.AppendLine();
        sb.AppendLine("```bash");
        sb.AppendLine("dotnet run");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("## Architecture");
        sb.AppendLine();
        sb.AppendLine("```mermaid");
        sb.AppendLine("graph TD");
        sb.AppendLine("    A[Application Entry] --> B[Core Logic]");
        sb.AppendLine("    B --> C[Output]");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("## License");
        sb.AppendLine();
        sb.AppendLine("MIT License");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("## Next Steps");
        sb.AppendLine("1. Review and customize documentation for project specifics");
        sb.AppendLine("2. Add screenshots or demo videos if applicable");
        sb.AppendLine("3. Enable XML documentation generation in .csproj:");
        sb.AppendLine("   ```xml");
        sb.AppendLine("   <GenerateDocumentationFile>true</GenerateDocumentationFile>");
        sb.AppendLine("   ```");

        return sb.ToString();
    }

    private static string BuildSoftwareArchitectOutput(string prompt)
    {
        var sb = new StringBuilder();
        var promptLower = prompt.ToLowerInvariant();

        sb.AppendLine("# Architecture Review");
        sb.AppendLine();
        sb.AppendLine("## Summary");
        sb.AppendLine("Overall architecture quality: **Good** — Clean, maintainable design with minor recommendations.");
        sb.AppendLine();
        sb.AppendLine("## Architecture Overview");
        sb.AppendLine();

        if (promptLower.Contains("api") && (promptLower.Contains("service") || promptLower.Contains("repository")))
        {
            sb.AppendLine("### Current Architecture Pattern");
            sb.AppendLine("**Layered Architecture** with clear separation of concerns.");
            sb.AppendLine();
            sb.AppendLine("### Layers Identified");
            sb.AppendLine("- **Presentation**: API Controllers/Endpoints");
            sb.AppendLine("- **Business Logic**: Services");
            sb.AppendLine("- **Data Access**: Repositories");
            sb.AppendLine();
            sb.AppendLine("## SOLID Principles Review");
            sb.AppendLine();
            sb.AppendLine("### ✅ Strengths");
            sb.AppendLine("- **Single Responsibility**: Each class has a clear, focused purpose");
            sb.AppendLine("- **Dependency Inversion**: Controllers depend on service interfaces");
            sb.AppendLine("- **Interface Segregation**: Interfaces are focused and cohesive");
            sb.AppendLine();
            sb.AppendLine("### 🟡 Recommendations");
            sb.AppendLine("- Consider extracting validation logic into a separate validator class");
            sb.AppendLine("- Use repository pattern to abstract data access");
        }
        else if (promptLower.Contains("api") || promptLower.Contains("web"))
        {
            sb.AppendLine("### Current Architecture Pattern");
            sb.AppendLine("**Minimal API** approach — appropriate for simple APIs and microservices.");
            sb.AppendLine();
            sb.AppendLine("### Components");
            sb.AppendLine("- **API Endpoints**: Inline route handlers");
            sb.AppendLine("- **Models**: Data transfer objects");
            sb.AppendLine();
            sb.AppendLine("## SOLID Principles Review");
            sb.AppendLine();
            sb.AppendLine("### ✅ Strengths");
            sb.AppendLine("- Minimal complexity for simple use case");
            sb.AppendLine("- Clean, readable code");
            sb.AppendLine("- Appropriate pattern for microservices");
            sb.AppendLine();
            sb.AppendLine("### 🟡 Recommendations for Growth");
            sb.AppendLine("- If API grows beyond 10 endpoints, consider extracting to service classes");
            sb.AppendLine("- Add repository pattern when database complexity increases");
            sb.AppendLine("- Consider adding input validation with FluentValidation");
        }
        else
        {
            sb.AppendLine("### Current Architecture Pattern");
            sb.AppendLine("**Simple Console Application** — appropriate for the current scope.");
            sb.AppendLine();
            sb.AppendLine("## SOLID Principles Review");
            sb.AppendLine();
            sb.AppendLine("### ✅ Strengths");
            sb.AppendLine("- Clear entry point and execution flow");
            sb.AppendLine("- Appropriate complexity for requirements");
            sb.AppendLine();
            sb.AppendLine("### 🟡 Recommendations for Growth");
            sb.AppendLine("- Extract business logic from Program.cs if application grows");
            sb.AppendLine("- Consider dependency injection if external dependencies increase");
        }

        sb.AppendLine();
        sb.AppendLine("## Design Patterns");
        sb.AppendLine();
        sb.AppendLine("### Patterns Used Correctly");
        sb.AppendLine("- ✅ Dependency Injection (ASP.NET Core DI container)");

        if (promptLower.Contains("record"))
        {
            sb.AppendLine("- ✅ Immutable Records for DTOs");
        }

        sb.AppendLine();
        sb.AppendLine("## Scalability and Performance");
        sb.AppendLine();
        sb.AppendLine("### ✅ Async/Await Usage");
        sb.AppendLine("- I/O operations should be async for better scalability");
        sb.AppendLine();
        sb.AppendLine("### Caching Opportunities");
        sb.AppendLine("- Consider caching frequently accessed data");
        sb.AppendLine("- Use `IMemoryCache` for in-process caching");
        sb.AppendLine();
        sb.AppendLine("## Recommendations");
        sb.AppendLine();
        sb.AppendLine("### High Priority");
        sb.AppendLine("None — current architecture is appropriate for scope.");
        sb.AppendLine();
        sb.AppendLine("### Medium Priority");
        sb.AppendLine("1. Add logging with `ILogger<T>` for observability");
        sb.AppendLine("2. Implement health checks for production deployments");
        sb.AppendLine();
        sb.AppendLine("### Low Priority");
        sb.AppendLine("1. Consider OpenAPI/Swagger documentation for APIs");
        sb.AppendLine("2. Add metrics collection for monitoring");
        sb.AppendLine();
        sb.AppendLine("## Long-Term Maintainability Score: 8/10");
        sb.AppendLine();
        sb.AppendLine("### Factors");
        sb.AppendLine("- **Separation of Concerns**: 8/10");
        sb.AppendLine("- **SOLID Compliance**: 8/10");
        sb.AppendLine("- **Testability**: 7/10");
        sb.AppendLine("- **Scalability**: 8/10");
        sb.AppendLine("- **Documentation**: 6/10 (can be improved)");

        return sb.ToString();
    }
}
