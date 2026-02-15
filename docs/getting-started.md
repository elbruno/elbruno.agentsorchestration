# Getting Started

This guide teaches you how to use the **ElBruno.AgentsOrchestration** libraries by starting with the simplest possible code and progressively adding features. By the end, you'll understand how to build, customize, and monitor multi-agent orchestration systems.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

Verify your setup:

```bash
dotnet --version         # should print 10.x
```

## Your First Orchestration

Create a new console app and add the Core package:

```bash
dotnet new console -n MyFirstOrchestration
cd MyFirstOrchestration
dotnet add package ElBruno.AgentsOrchestration.Core
```

Replace `Program.cs` with this minimal code:

```csharp
using ElBruno.AgentsOrchestration.Core.Orchestration;
using ElBruno.AgentsOrchestration.Orchestration;

var service = OrchestrationServiceFactory.Create();
var result = await service.RunAsync(
    new OrchestrationRequest("Create a .NET console app that prints 'Hello, World!'"),
    CancellationToken.None
);

Console.WriteLine($"✅ Done! Workspace: {result.WorkspacePath}");
```

Run it:

```bash
dotnet run
```

The orchestration engine will create a workspace directory with a working .NET console app. That's it — **3 lines of code** to run a full multi-agent orchestration!

## Understanding What Happened

When you call `RunAsync()`, the library executes a **6-step pipeline**:

1. **Plan** — The Planner agent analyzes your prompt and creates an execution plan
2. **Parse** — The plan is broken into phases and tasks
3. **Execute** — Coder and Designer agents generate files in parallel
4. **Verify** — The Orchestrator runs `dotnet build` to validate the output
5. **Review** — The BuildReviewer checks code quality (optional)
6. **Report** — A summary is returned with workspace path and task results

If the build fails, the **Fixer** agent automatically analyzes errors and corrects the code (up to 3 attempts by default).

## Adding Event Streaming

To see what's happening in real-time, listen to the event stream:

```csharp
using ElBruno.AgentsOrchestration.Core.Orchestration;
using ElBruno.AgentsOrchestration.Orchestration;

var service = OrchestrationServiceFactory.Create();

// Start reading events in the background
var eventTask = Task.Run(async () =>
{
    await foreach (var evt in service.Events.Reader.ReadAllAsync())
    {
        var message = evt switch
        {
            OrchestrationStartedEvent => "🚀 Started",
            PhaseStartedEvent phase => $"📌 Phase {phase.PhaseIndex}: {phase.PhaseName}",
            AgentActivatedEvent agent => $"🤖 {agent.Role} working...",
            FileCreatedEvent file => $"📄 Created: {Path.GetFileName(file.FilePath)}",
            BuildValidationEvent build => build.Success ? "✅ Build passed" : "❌ Build failed",
            OrchestrationCompletedEvent => "🏁 Complete",
            _ => null
        };
        if (message is not null)
            Console.WriteLine(message);
    }
});

// Run orchestration
var result = await service.RunAsync(
    new OrchestrationRequest("Create a weather console app"),
    CancellationToken.None
);

// Close the event channel and wait for the reader
service.Events.Writer.TryComplete();
await eventTask;

Console.WriteLine($"\n✅ Done! Workspace: {result.WorkspacePath}");
```

Now you'll see real-time progress as agents activate, create files, and validate builds.

## Saving and Viewing Output

Each orchestration creates a timestamped workspace directory. List the generated files:

```csharp
var result = await service.RunAsync(
    new OrchestrationRequest("Create a to-do API"),
    CancellationToken.None
);

Console.WriteLine($"\n📁 Generated files in: {result.WorkspacePath}\n");

var files = Directory.GetFiles(result.WorkspacePath, "*", SearchOption.AllDirectories)
    .Select(f => Path.GetRelativePath(result.WorkspacePath, f))
    .OrderBy(f => f);

foreach (var file in files)
{
    Console.WriteLine($"  📄 {file}");
}
```

## Running Generated Apps

Show users how to run the generated application:

```csharp
var result = await service.RunAsync(
    new OrchestrationRequest("Create a simple calculator console app"),
    CancellationToken.None
);

// Find the .csproj file
var projectFile = Directory.GetFiles(result.WorkspacePath, "*.csproj", SearchOption.TopDirectoryOnly)
    .FirstOrDefault();

if (projectFile is not null)
{
    Console.WriteLine($"\n📋 To run the generated app:\n");
    Console.WriteLine($"  dotnet run --project \"{projectFile}\"\n");
}
```

## Multi-Turn Conversations

For interactive scenarios where users want to make iterative changes, use `ConversationManager`:

```csharp
using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.Core.Orchestration;
using ElBruno.AgentsOrchestration.Orchestration;
using ElBruno.AgentsOrchestration.Workspace;

// Initialize components
var instructions = InstructionLoader.LoadInstructions();
var store = new AgentConfigurationStore(instructions);
var client = new TemplateAgentClient();
var factory = new AgentFactory(store, client);
var workspaceManager = new WorkspaceManager("./workspaces");
var conversationManager = new ConversationManager();

// First turn - create initial app
var workspace1 = workspaceManager.CreateWorkspace("Create a weather app");
var session = conversationManager.CreateSession(workspace1);

var service = new OrchestrationService(factory, workspaceManager);
var result1 = await service.RunAsync(
    new OrchestrationRequest("Create a weather console app for London"),
    CancellationToken.None
);

// Record the turn
session = conversationManager.RecordTurn(session.SessionId, 
    "Create a weather console app for London", 
    result1);

// Second turn - add features with context
var contextPrompt = session.BuildContextPrompt("Add Tokyo and New York to the cities");
var result2 = await service.RunAsync(
    new OrchestrationRequest(contextPrompt),
    CancellationToken.None
);

Console.WriteLine($"Session has {session.History.Count} turns");
```

The `ConversationManager` tracks conversation history and builds context-aware prompts so agents can make incremental changes.

## Launching Apps Programmatically

The `AppRunner` class lets you launch generated apps as background processes and stream their output:

```csharp
using ElBruno.AgentsOrchestration.Workspace;

var result = await service.RunAsync(
    new OrchestrationRequest("Create a web server on port 5000"),
    CancellationToken.None
);

// Launch the app
var appRunner = new AppRunner();
appRunner.OnLogReceived += line => Console.WriteLine($"[App] {line}");

if (await appRunner.LaunchAsync(result.WorkspacePath, CancellationToken.None))
{
    Console.WriteLine("✅ App is running. Press Enter to stop...");
    Console.ReadLine();
    await appRunner.StopAsync();
}
```

This is perfect for web servers, long-running services, or any app that needs to be monitored.

## Visualizing Agent Flows

For debugging and understanding agent interactions, use the `AgentCallGraph` to visualize the orchestration flow:

```csharp
using ElBruno.AgentsOrchestration.Views;

var callGraph = new AgentCallGraph();
var service = OrchestrationServiceFactory.Create();

// Track events
var eventTask = Task.Run(async () =>
{
    await foreach (var evt in service.Events.Reader.ReadAllAsync())
    {
        // Record agent calls
        if (evt is AgentActivatedEvent active)
        {
            callGraph.RecordCall(
                AgentRole.Orchestrator, 
                active.Role, 
                active.TaskDescription, 
                DateTimeOffset.UtcNow
            );
        }
    }
});

var result = await service.RunAsync(
    new OrchestrationRequest("Create a Blazor recipe app"),
    CancellationToken.None
);

service.Events.Writer.TryComplete();
await eventTask;

// Display the flow
Console.WriteLine("\n📊 Agent Interaction Flow:");
Console.WriteLine(callGraph.ToAsciiFlow());
```

This produces an ASCII diagram showing which agents were called, in what order, and for what tasks.

## Customizing Configuration

Configure workspace location, fix attempts, and more:

```csharp
var service = OrchestrationServiceFactory.Create(options =>
{
    options.WorkspaceRoot = "./my-workspaces";     // Custom workspace directory
    options.MaxFixAttempts = 5;                     // More fix retries
});
```

Or customize agent instructions:

```csharp
var customInstructions = new Dictionary<AgentRole, string>
{
    [AgentRole.Coder] = "Write C# code using minimal APIs and record types.",
    [AgentRole.Designer] = "Use Tailwind CSS for all styling."
};

var service = OrchestrationServiceFactory.Create(options =>
{
    options.CustomInstructions = customInstructions;
});
```

## Using Dependency Injection

For ASP.NET Core, Blazor, or .NET Aspire applications, use the `AddOrchestration()` extension method:

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register orchestration services
builder.Services.AddOrchestration(options =>
{
    options.WorkspaceRoot = "./workspaces";
    options.MaxFixAttempts = 3;
});

var app = builder.Build();

// Use in a minimal API endpoint
app.MapPost("/orchestrate", async (
    string prompt, 
    OrchestrationService service,
    CancellationToken ct) =>
{
    var result = await service.RunAsync(
        new OrchestrationRequest(prompt), 
        ct
    );
    return Results.Ok(result);
});

app.Run();
```

Or inject into controllers:

```csharp
public class OrchestrationController : ControllerBase
{
    private readonly OrchestrationService _service;
    
    public OrchestrationController(OrchestrationService service)
    {
        _service = service;
    }
    
    [HttpPost("run")]
    public async Task<IActionResult> Run([FromBody] string prompt)
    {
        var result = await _service.RunAsync(
            new OrchestrationRequest(prompt),
            HttpContext.RequestAborted
        );
        return Ok(result);
    }
}
```

### Service Lifetimes

The `AddOrchestration()` method registers services with appropriate lifetimes:

- **Singleton**: `AgentConfigurationStore`, `IAgentClient`, `AgentFactory` — shared across all requests
- **Scoped**: `OrchestrationService`, `IWorkspace` — new instance per HTTP request or scope

This ensures workspace isolation and proper resource management.

## Persisting Sessions

For long-running conversations, save and resume sessions:

```csharp
using ElBruno.AgentsOrchestration.Core.Orchestration;

var sessionPersistence = new SessionPersistence("./sessions");

// After orchestration, save the session
await sessionPersistence.SaveSessionAsync(session);

// Later, list and resume
var savedFiles = sessionPersistence.ListSavedSessionFiles();
var session = await sessionPersistence.LoadSessionAsync(savedFiles[0]);

Console.WriteLine($"Resumed session: {session.SessionId}");
Console.WriteLine($"Turns: {session.History.Count}");

// Export as markdown for documentation
await sessionPersistence.SaveAsMarkdownAsync(session, "conversation.md");
```

## Next Steps

Now that you understand the basics and advanced features, explore:

- **[Samples Overview](samples-overview.md)** — See working examples: ConsoleSimple, ConsoleCompleteChat, ConsoleFlowTraces, and AspireApp
- **[Using the Libraries](using-the-libraries.md)** — Deep dive into the three NuGet packages and integration patterns
- **[Architecture](architecture.md)** — Understand the 6-step pipeline and agent coordination
- **[Event Streaming](event-streaming.md)** — Complete reference for all 18+ event types
- **[Library Packages](library-packages.md)** — Package structure and versioning
- **[README](../README.md)** — Project overview and API reference

---

## 🎓 Want to Learn More?

Building AI orchestration systems is exciting! If you want to dive deeper into agent architecture, check out my content:

- **📝 [Detailed Blog Posts](https://elbruno.com)** — Technical deep-dives on orchestration patterns
- **▶️ [Video Tutorials](https://www.youtube.com/elbruno)** — Watch me build and explain the system live
- **🎙️ [Podcast Episodes](https://notienenombre.com)** — Interviews and discussions about AI agents (Spanish 🇪🇸)
- **💼 [LinkedIn insights](https://www.linkedin.com/in/elbruno/)** — Quick tips and updates

**Have questions or feedback?** Reach out on [Twitter/X](https://www.x.com/in/elbruno/) or [GitHub](https://github.com/elbruno/) — I love hearing from the community! 🙌
