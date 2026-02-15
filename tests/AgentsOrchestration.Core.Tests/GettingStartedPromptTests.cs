using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.Orchestration;
using ElBruno.AgentsOrchestration.Workspace;

namespace AgentsOrchestration.Core.Tests;

/// <summary>
/// Integration tests that run each sample prompt from the getting-started guide
/// through the full orchestration pipeline using the TemplateAgentClient (test mode).
/// </summary>
public class GettingStartedPromptTests
{
    private static async Task<(OrchestrationResult Result, IReadOnlyCollection<string> Files, List<OrchestrationEvent> Events)>
        RunPromptAsync(string prompt)
    {
        var root = Path.Combine(Path.GetTempPath(), $"gs-tests-{Guid.NewGuid():N}");
        try
        {
            var manager = new WorkspaceManager(root);
            var store = new AgentConfigurationStore();
            var factory = new AgentFactory(store, new TemplateAgentClient());
            var service = new OrchestrationService(factory, manager);

            var events = new List<OrchestrationEvent>();
            var readTask = Task.Run(async () =>
            {
                await foreach (var evt in service.Events.Reader.ReadAllAsync())
                {
                    events.Add(evt);
                }
            });

            var result = await service.RunAsync(new OrchestrationRequest(prompt), CancellationToken.None);
            service.Events.Writer.TryComplete();
            await readTask;

            var files = manager.ListFiles();
            return (result, files, events);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public async Task BlazorRecipeApp_CompletesSuccessfully()
    {
        var prompt = "Create a Blazor Server app that manages cooking recipes. Include a Recipe model "
            + "with name, ingredients, and instructions. Add pages to list all recipes, view "
            + "details, and create or edit a recipe. Use Bootstrap for styling.";

        var (result, files, events) = await RunPromptAsync(prompt);

        Assert.NotEmpty(result.TaskResults);
        Assert.NotEmpty(files);
        Assert.Contains(events, e => e is OrchestrationStartedEvent);
        Assert.Contains(events, e => e is OrchestrationCompletedEvent);
    }

    [Fact]
    public async Task StaticLandingPage_CompletesSuccessfully()
    {
        var prompt = "Build a responsive landing page for a SaaS product called \"TaskFlow\". Include a "
            + "hero section with a call-to-action button, a features grid with three cards, a "
            + "pricing table with Free, Pro, and Enterprise tiers, and a footer with social links. "
            + "Use plain HTML and CSS only.";

        var (result, files, events) = await RunPromptAsync(prompt);

        Assert.NotEmpty(result.TaskResults);
        Assert.NotEmpty(files);
        Assert.Contains(events, e => e is OrchestrationStartedEvent);
        Assert.Contains(events, e => e is OrchestrationCompletedEvent);
    }

    [Fact]
    public async Task RestApi_CompletesSuccessfully()
    {
        var prompt = "Create a minimal ASP.NET Core Web API for a to-do list. Include endpoints to "
            + "list, create, update, and delete to-do items. Store items in-memory using a "
            + "ConcurrentDictionary. Return JSON with proper HTTP status codes.";

        var (result, files, events) = await RunPromptAsync(prompt);

        Assert.NotEmpty(result.TaskResults);
        Assert.NotEmpty(files);
        Assert.Contains(events, e => e is OrchestrationStartedEvent);
        Assert.Contains(events, e => e is OrchestrationCompletedEvent);
    }

    [Fact]
    public async Task ConsoleUtility_CompletesSuccessfully()
    {
        var prompt = "Build a .NET console app that reads a CSV file, groups rows by a user-specified "
            + "column, and prints a summary table with counts per group. Use Spectre.Console for "
            + "the table output.";

        var (result, files, events) = await RunPromptAsync(prompt);

        Assert.NotEmpty(result.TaskResults);
        Assert.NotEmpty(files);
        Assert.Contains(events, e => e is OrchestrationStartedEvent);
        Assert.Contains(events, e => e is OrchestrationCompletedEvent);
    }

    [Fact]
    public async Task WeatherDashboard_CompletesSuccessfully()
    {
        var prompt = "Create a Blazor Server dashboard that shows real-time weather for three cities "
            + "(London, Tokyo, New York). Use a WeatherService that returns random temperatures, "
            + "a card per city, and auto-refresh every 5 seconds with a Timer.";

        var (result, files, events) = await RunPromptAsync(prompt);

        Assert.NotEmpty(result.TaskResults);
        Assert.NotEmpty(files);
        Assert.Contains(events, e => e is OrchestrationStartedEvent);
        Assert.Contains(events, e => e is OrchestrationCompletedEvent);
    }

    [Fact]
    public async Task FullStackInventory_CompletesSuccessfully()
    {
        var prompt = "Build a .NET solution with two projects. First, an ASP.NET Core minimal API "
            + "(\"InventoryApi\") with CRUD endpoints for products (Id, Name, Sku, Price, "
            + "Quantity). Use Entity Framework Core with an in-memory database. Second, a "
            + "Blazor Server app (\"InventoryDashboard\") that consumes the API with HttpClient, "
            + "displays products in a searchable DataGrid, and has forms for add/edit. Share "
            + "a \"Contracts\" class library between both projects for DTOs.";

        var (result, files, events) = await RunPromptAsync(prompt);

        Assert.NotEmpty(result.TaskResults);
        Assert.NotEmpty(files);
        Assert.Contains(events, e => e is OrchestrationStartedEvent);
        Assert.Contains(events, e => e is OrchestrationCompletedEvent);
    }

    [Fact]
    public async Task TemplateClient_ReturnsProperPlan_ForAllPrompts()
    {
        var client = new TemplateAgentClient();
        var prompts = new[]
        {
            "Create a Blazor Server app that manages cooking recipes.",
            "Build a responsive landing page for a SaaS product.",
            "Create a minimal ASP.NET Core Web API for a to-do list.",
            "Build a .NET console app that reads a CSV file.",
            "Create a Blazor Server dashboard for weather.",
            "Build a .NET solution with two projects."
        };

        foreach (var prompt in prompts)
        {
            var planOutput = await client.RunAsync(AgentRole.Planner, prompt, "/ws", CancellationToken.None);
            var plan = OrchestrationService.ParsePlan(planOutput, prompt);

            Assert.NotEmpty(plan.Phases);
            foreach (var phase in plan.Phases)
            {
                Assert.NotEmpty(phase.Tasks);
                foreach (var task in phase.Tasks)
                {
                    Assert.False(string.IsNullOrWhiteSpace(task.Description), $"Empty description in plan for prompt: {prompt}");
                    Assert.False(string.IsNullOrWhiteSpace(task.FileScope), $"Empty file scope in plan for prompt: {prompt}");
                }
            }
        }
    }

    [Fact]
    public async Task TemplateClient_CoderOutput_IsNonEmpty_ForAllPrompts()
    {
        var client = new TemplateAgentClient();
        var taskPrompts = new[]
        {
            ("Create project file for: Create a Blazor Server app that manages cooking recipes.", "csproj"),
            ("Implement main application logic for: Create a Blazor Server app that manages cooking recipes.", "program"),
            ("Create data models for: Create a Blazor Server app that manages cooking recipes.", "models"),
            ("Create project file for: Create a minimal ASP.NET Core Web API for a to-do list.", "csproj"),
            ("Implement main application logic for: Create a minimal ASP.NET Core Web API for a to-do list.", "program"),
            ("Create data models for: Create a minimal ASP.NET Core Web API for a to-do list.", "models"),
            ("Create project file for: Build a .NET console app that reads a CSV file, groups rows by a user-specified column.", "csproj"),
            ("Implement main application logic for: Build a .NET console app that reads a CSV file, groups rows by a user-specified column.", "program"),
        };

        foreach (var (prompt, label) in taskPrompts)
        {
            var result = await client.RunAsync(AgentRole.Coder, prompt, "/ws", CancellationToken.None);
            Assert.False(string.IsNullOrWhiteSpace(result), $"Coder produced empty output for {label} task: {prompt}");
        }
    }

    [Fact]
    public async Task TemplateClient_TodoModels_RecognizesToDoWithHyphen()
    {
        var client = new TemplateAgentClient();

        // The REST API prompt uses "to-do" (with hyphen), not "todo"
        var result = await client.RunAsync(
            AgentRole.Coder,
            "Create data models for: Create a minimal ASP.NET Core Web API for a to-do list.",
            "/ws",
            CancellationToken.None);

        // The models should contain TodoItem, not the generic AppModel
        Assert.Contains("TodoItem", result);
    }
}
