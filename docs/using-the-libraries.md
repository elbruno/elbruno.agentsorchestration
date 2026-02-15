# Using the AgentsOrchestration Libraries

This guide walks you through using the three **AgentsOrchestration** NuGet packages to build your own multi-agent orchestration system with the Microsoft Agent Framework. Each section adds a layer — start with the one that matches your needs.

## Package Overview

```
AgentsOrchestration.Abstractions     ← Zero dependencies (pure types)
    ↑
AgentsOrchestration.Orchestration    ← 6-step pipeline engine
    ↑
AgentsOrchestration.Core             ← Batteries-included (template client + workspace)
```

| Package | Use when… |
|---------|-----------|
| **Abstractions** | You want to define agents and plug in your own LLM provider |
| **Orchestration** | You want the full multi-agent pipeline with event streaming |
| **Core** | You want everything ready to go for learning and experimentation |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

Verify your setup:

```bash
dotnet --version   # should print 10.x
```

---

## Quick Start — Simplified API (Recommended)

For most use cases, start with the simplified API in the **Core** package:

### Static Factory Method (Console Apps)

```csharp
using ElBruno.AgentsOrchestration.Core.Orchestration;
using ElBruno.AgentsOrchestration.Orchestration;

// One line to create the service
var service = OrchestrationServiceFactory.Create();

// Run orchestration
var result = await service.RunAsync(
    new OrchestrationRequest("Create a .NET weather console app"),
    CancellationToken.None
);

Console.WriteLine($"✅ Done! Workspace: {result.WorkspacePath}");
```

### Dependency Injection (ASP.NET / Aspire)

```csharp
using Microsoft.Extensions.DependencyInjection;

// In Program.cs
builder.Services.AddOrchestration(options =>
{
    options.WorkspaceRoot = "workspaces";
    options.MaxFixAttempts = 3;
});

// Then inject
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

### Configuration Options

```csharp
var service = OrchestrationServiceFactory.Create(options =>
{
    options.WorkspaceRoot = "custom-path";         // Where workspaces are created
    options.MaxFixAttempts = 5;                    // Retry count for build failures
    options.CustomClient = new MyLlmClient();      // Override default agent client
    options.CustomInstructions = myInstructions;   // Override agent instructions
});
```

**OrchestrationOptions properties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `WorkspaceRoot` | `string` | temp directory | Root path for workspace creation |
| `MaxFixAttempts` | `int` | `3` | Number of self-healing attempts on build failure |
| `CustomClient` | `IAgentClient?` | `null` | Custom LLM client (uses `TemplateAgentClient` if null) |
| `CustomInstructions` | `IReadOnlyDictionary<AgentRole, string>?` | `null` | Custom agent instructions (loads from files/defaults if null) |

**That's all you need!** The factory automatically:

- Loads agent instructions (from files, environment, or built-in defaults)
- Creates the configuration store
- Initializes the agent factory
- Sets up workspace management

Continue reading for advanced scenarios and custom integrations.

---

## Scenario 1 — Use Abstractions Only

**Goal:** Define agents and connect your own LLM to the `IAgentClient` interface.

### Step 1 — Create a new console project

```bash
dotnet new console -n MyAgentApp
cd MyAgentApp
```

### Step 2 — Add the Abstractions package

```bash
dotnet add package AgentsOrchestration.Abstractions
```

### Step 3 — Implement `IAgentClient`

Create a class that calls your LLM provider. The interface has a single method:

```csharp
using ElBruno.AgentsOrchestration.Agents;

public sealed class MyLlmClient : IAgentClient
{
    public async Task<string> RunAsync(
        AgentRole role,
        string prompt,
        string workspacePath,
        CancellationToken cancellationToken)
    {
        // Replace with your actual LLM call (OpenAI, Azure OpenAI, Ollama, etc.)
        Console.WriteLine($"[{role}] Received prompt: {prompt[..Math.Min(prompt.Length, 80)]}...");

        // Simulate LLM response
        await Task.Delay(100, cancellationToken);
        return $"Response from {role} for: {prompt}";
    }
}
```

### Step 4 — Wire up agents and run a session

```csharp
using ElBruno.AgentsOrchestration.Agents;

// 1. Create the configuration store with default instructions
var store = new AgentConfigurationStore();

// 2. Create the factory with your LLM client
var factory = new AgentFactory(store, new MyLlmClient());

// 3. Create a session for any agent role
var plannerSession = factory.CreateSession(AgentRole.Planner);
Console.WriteLine($"Agent: {plannerSession.Configuration.Name}");
Console.WriteLine($"Model: {plannerSession.Configuration.Model}");
Console.WriteLine($"Icon:  {plannerSession.Configuration.Icon}");

// 4. Run the agent
var result = await plannerSession.RunAsync(
    "Create a REST API for managing tasks",
    "/tmp/workspace",
    CancellationToken.None);

Console.WriteLine($"\nPlanner output:\n{result}");
```

### Step 5 — Customize agent instructions

You can provide custom instructions in two ways:

**Option A — Programmatic:**

```csharp
var customInstructions = new Dictionary<AgentRole, string>
{
    [AgentRole.Orchestrator] = "You coordinate the team. Be concise.",
    [AgentRole.Planner]      = "Create step-by-step plans in Markdown format.",
    [AgentRole.Coder]        = "Write idiomatic C# code targeting .NET 10.",
    [AgentRole.Designer]     = "Focus on accessibility and responsive design.",
    [AgentRole.Fixer]        = "Analyze build errors and fix only what is broken."
};

var store = new AgentConfigurationStore(customInstructions);
```

**Option B — Load from Markdown files:**

Create a directory with one `.md` file per role:

```
Instructions/
  orchestrator.md
  planner.md
  coder.md
  designer.md
  fixer.md
```

Then load them:

```csharp
var instructions = InstructionLoader.LoadFromDirectory("./Instructions");
var store = new AgentConfigurationStore(instructions);
```

### Step 6 — Update agent configuration at runtime

```csharp
// Get the current configuration
var coderConfig = store.Get(AgentRole.Coder);
Console.WriteLine($"Current model: {coderConfig.Model}");

// Update instructions
store.Update(coderConfig with { Instructions = "Write TypeScript instead of C#." });

// Reset to defaults
store.ResetInstructions(AgentRole.Coder);
```

---

## Scenario 2 — Use the Orchestration Engine

**Goal:** Run the full 6-step pipeline (Plan → Parse → Execute → Verify → Review → Report) with event streaming.

### Step 1 — Create a new console project

```bash
dotnet new console -n MyOrchestrator
cd MyOrchestrator
```

### Step 2 — Add the Orchestration package

```bash
dotnet add package AgentsOrchestration.Orchestration
```

> This automatically brings in `AgentsOrchestration.Abstractions` as a transitive dependency.

### Step 3 — Implement `IAgentClient` and `IWorkspace`

The orchestration engine needs two things you provide:

1. **`IAgentClient`** — how to call your LLM
2. **`IWorkspace`** — where to create workspace directories

```csharp
using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.Orchestration;

// Your LLM client (same as Scenario 1)
public sealed class MyLlmClient : IAgentClient
{
    public async Task<string> RunAsync(
        AgentRole role,
        string prompt,
        string workspacePath,
        CancellationToken cancellationToken)
    {
        await Task.Delay(50, cancellationToken);

        // Return role-specific template output
        return role switch
        {
            AgentRole.Planner => """
                ## Phase 1: Setup
                - Task: Create project file | Agent: Coder | File: project.csproj
                ## Phase 2: Implementation
                - Task: Implement main logic | Agent: Coder | File: Program.cs
                - Task: Create styles | Agent: Designer | File: styles.css
                """,
            AgentRole.Coder => $"// Generated code for: {prompt}\nConsole.WriteLine(\"Hello!\");",
            AgentRole.Designer => $"/* Styles for: {prompt} */\nbody {{ font-family: sans-serif; }}",
            _ => $"Completed: {prompt}"
        };
    }
}

// Your workspace strategy
public sealed class SimpleWorkspace : IWorkspace
{
    public string CreateWorkspace(string prompt)
    {
        var dir = Path.Combine(Path.GetTempPath(), $"orchestration-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        return dir;
    }
}
```

### Step 4 — Run the orchestration pipeline

```csharp
using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.Orchestration;

// Wire up
var store = new AgentConfigurationStore();
var factory = new AgentFactory(store, new MyLlmClient());
var workspace = new SimpleWorkspace();
var service = new OrchestrationService(factory, workspace, maxFixAttempts: 2);

// Run the pipeline
var request = new OrchestrationRequest("Build a to-do API with CRUD endpoints");
var result = await service.RunAsync(request, CancellationToken.None);

// Inspect results
Console.WriteLine($"Summary: {result.Summary}");
Console.WriteLine($"Tasks completed: {result.TaskResults.Count}");
foreach (var task in result.TaskResults)
{
    Console.WriteLine($"  [{task.Role}] {task.Description}");
}
```

### Step 5 — Stream events in real time

The `OrchestrationService.Events` channel lets you observe every step of the pipeline as it happens:

```csharp
using System.Threading.Channels;
using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.Orchestration;

var store = new AgentConfigurationStore();
var factory = new AgentFactory(store, new MyLlmClient());
var workspace = new SimpleWorkspace();
var service = new OrchestrationService(factory, workspace);

// Start reading events in the background
var eventReader = Task.Run(async () =>
{
    await foreach (var evt in service.Events.Reader.ReadAllAsync())
    {
        var message = evt switch
        {
            OrchestrationStartedEvent started
                => $"🚀 Started: {started.Prompt}",
            PhaseStartedEvent phase
                => $"📋 Phase {phase.PhaseIndex}: {phase.PhaseName}",
            AgentActivatedEvent activated
                => $"⚡ {activated.Role} working on: {activated.TaskDescription}",
            AgentCompletedEvent completed
                => $"✅ {completed.Role} finished",
            FileCreatedEvent file
                => $"📄 File created: {file.FilePath}",
            BuildValidationEvent build
                => build.Success ? "✅ Build passed" : $"❌ Build failed: {build.Output}",
            FixAttemptStartedEvent fix
                => $"🔧 Fix attempt {fix.Attempt}",
            OrchestrationCompletedEvent done
                => $"🎉 Done: {done.FinalResult.Summary}",
            OrchestrationErrorEvent error
                => $"💥 Error: {error.Error}",
            _ => $"📨 {evt.GetType().Name}"
        };
        Console.WriteLine($"[{evt.Timestamp:HH:mm:ss}] {message}");
    }
});

// Run the orchestration
var result = await service.RunAsync(
    new OrchestrationRequest("Create a weather dashboard"),
    CancellationToken.None);

// Signal the event channel is done and wait for the reader
service.Events.Writer.TryComplete();
await eventReader;
```

### Step 6 — Use events with SignalR (ASP.NET Core)

Here's how to relay orchestration events to a web frontend via SignalR:

```csharp
using Microsoft.AspNetCore.SignalR;
using ElBruno.AgentsOrchestration.Orchestration;

public sealed class OrchestrationHub : Hub
{
    private readonly IServiceScopeFactory _scopeFactory;

    public OrchestrationHub(IServiceScopeFactory scopeFactory)
        => _scopeFactory = scopeFactory;

    public async Task StartOrchestration(string prompt)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider
            .GetRequiredService<OrchestrationService>();

        // Stream events to the caller
        var readEvents = Task.Run(async () =>
        {
            await foreach (var evt in service.Events.Reader.ReadAllAsync())
            {
                await Clients.Caller.SendAsync("OrchestrationEvent", new
                {
                    Type = evt.GetType().Name,
                    Timestamp = evt.Timestamp
                });
            }
        });

        await service.RunAsync(
            new OrchestrationRequest(prompt),
            CancellationToken.None);

        service.Events.Writer.TryComplete();
        await readEvents;
    }
}
```

---

## Scenario 3 — Use Core (Batteries Included)

**Goal:** Get started quickly with the template agent client and workspace manager — no LLM required.

### Step 1 — Create a new console project

```bash
dotnet new console -n QuickStart
cd QuickStart
```

### Step 2 — Add the Core package

```bash
dotnet add package AgentsOrchestration.Core
```

> This brings in both `Abstractions` and `Orchestration` as transitive dependencies, plus the `TemplateAgentClient`, `WorkspaceManager`, and Microsoft Agent Framework SDKs.

### Step 3 — Run an orchestration with zero configuration

```csharp
using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.Orchestration;
using ElBruno.AgentsOrchestration.Workspace;

// TemplateAgentClient returns deterministic output — no LLM needed
var store = new AgentConfigurationStore();
var factory = new AgentFactory(store, new TemplateAgentClient());
var workspace = new WorkspaceManager("./workspaces");
var service = new OrchestrationService(factory, workspace);

// Stream events to the console
var eventReader = Task.Run(async () =>
{
    await foreach (var evt in service.Events.Reader.ReadAllAsync())
    {
        Console.WriteLine($"[{evt.Timestamp:HH:mm:ss}] {evt.GetType().Name}");
    }
});

// Run the full pipeline
var result = await service.RunAsync(
    new OrchestrationRequest("Create a Blazor recipe app"),
    CancellationToken.None);

service.Events.Writer.TryComplete();
await eventReader;

// Show results
Console.WriteLine($"\n=== Orchestration Complete ===");
Console.WriteLine($"Summary: {result.Summary}");
Console.WriteLine($"Tasks: {result.TaskResults.Count}");

// List generated files
Console.WriteLine($"\nGenerated files:");
foreach (var file in workspace.ListFiles())
{
    Console.WriteLine($"  {file}");
}
```

### Step 4 — Register services with dependency injection

In an ASP.NET Core app (Blazor, Minimal API, etc.):

```csharp
using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.Orchestration;
using ElBruno.AgentsOrchestration.Workspace;

var builder = WebApplication.CreateBuilder(args);

// Agent infrastructure (singletons)
builder.Services.AddSingleton(sp =>
{
    var instructionsDir = Path.Combine(
        builder.Environment.ContentRootPath, "Instructions");
    return new AgentConfigurationStore(
        InstructionLoader.LoadFromDirectory(instructionsDir));
});
builder.Services.AddSingleton<IAgentClient, TemplateAgentClient>();
builder.Services.AddSingleton<AgentFactory>();

// Workspace and orchestration (scoped — one per request/run)
builder.Services.AddScoped(sp =>
{
    var root = builder.Configuration["Workspace:RootPath"] ?? "./workspaces";
    return new WorkspaceManager(Path.GetFullPath(root));
});
builder.Services.AddScoped<IWorkspace>(sp =>
    sp.GetRequiredService<WorkspaceManager>());
builder.Services.AddScoped(sp =>
{
    var maxFix = builder.Configuration
        .GetValue("Orchestration:MaxFixAttempts", 3);
    return new OrchestrationService(
        sp.GetRequiredService<AgentFactory>(),
        sp.GetRequiredService<IWorkspace>(),
        maxFix);
});
```

### Step 5 — Swap in a real LLM provider

When you're ready to connect a real LLM, replace `TemplateAgentClient` with your own implementation:

```csharp
// Your custom LLM client
public sealed class OpenAiAgentClient : IAgentClient
{
    private readonly HttpClient _httpClient;

    public OpenAiAgentClient(HttpClient httpClient)
        => _httpClient = httpClient;

    public async Task<string> RunAsync(
        AgentRole role,
        string prompt,
        string workspacePath,
        CancellationToken cancellationToken)
    {
        // Build the request using the agent's role to select behavior
        // The role tells you WHAT kind of output to produce
        // The prompt tells you WHAT to produce it for

        // Example: call your LLM API here
        var response = await CallLlmAsync(role, prompt, cancellationToken);
        return response;
    }

    private Task<string> CallLlmAsync(
        AgentRole role, string prompt, CancellationToken ct)
    {
        // Your LLM integration goes here
        throw new NotImplementedException();
    }
}

// Register it instead of TemplateAgentClient
builder.Services.AddSingleton<IAgentClient, OpenAiAgentClient>();
```

---

## Advanced Features in Core

The **Core** package includes several powerful features for building production-ready orchestration systems.

### Multi-Turn Conversations with ConversationManager

Enable context-aware conversations where each prompt builds on previous turns:

```csharp
using ElBruno.AgentsOrchestration.Core.Orchestration;

var conversationManager = new ConversationManager();

// Create a session
var workspace = workspaceManager.CreateWorkspace("Build a weather app");
var session = conversationManager.CreateSession(workspace);

// First turn
var result1 = await service.RunAsync(
    new OrchestrationRequest("Create a weather console app for London"),
    CancellationToken.None
);

// Record the turn
session = conversationManager.RecordTurn(session.SessionId, 
    "Create a weather console app for London", 
    result1);

// Second turn - add features with context
var contextPrompt = session.BuildContextPrompt("Add Tokyo and New York");
var result2 = await service.RunAsync(
    new OrchestrationRequest(contextPrompt),
    CancellationToken.None
);

Console.WriteLine($"Session has {session.History.Count} turns");
Console.WriteLine($"Estimated tokens: ~{session.EstimateTokenCount()}");
```

**Key methods:**

- `CreateSession(workspacePath)` — Start a new conversation
- `RecordTurn(sessionId, prompt, result)` — Add a turn to the history
- `GetSession(sessionId)` — Retrieve an existing session
- `ListSessions()` — Get all active sessions
- `ClearSession(sessionId)` — Remove a session from memory
- `session.BuildContextPrompt(newPrompt)` — Create a context-aware prompt

### Persisting Sessions with SessionPersistence

Save and restore conversation sessions across restarts:

```csharp
using ElBruno.AgentsOrchestration.Core.Orchestration;

var sessionPersistence = new SessionPersistence("./sessions");

// Save a session to disk
await sessionPersistence.SaveSessionAsync(session);

// List saved sessions
var savedFiles = sessionPersistence.ListSavedSessionFiles();

// Load a session
var loadedSession = await sessionPersistence.LoadSessionAsync(savedFiles[0]);

// Export as markdown for documentation
await sessionPersistence.SaveAsMarkdownAsync(session, "conversation.md");
```

Sessions are saved as JSON files: `session_{sessionId}_{timestamp}.json`

### Running Generated Apps with AppRunner

Launch generated applications and stream their output:

```csharp
using ElBruno.AgentsOrchestration.Workspace;

var appRunner = new AppRunner();

// Subscribe to log events
appRunner.OnLogReceived += line => Console.WriteLine($"[App] {line}");

// Launch the app (finds .csproj and runs it)
if (await appRunner.LaunchAsync(workspacePath, CancellationToken.None))
{
    Console.WriteLine("✅ App is running. Press Enter to stop...");
    Console.ReadLine();
    await appRunner.StopAsync();
}

appRunner.Dispose();
```

**Key features:**

- Automatically finds `.csproj` files
- Runs `dotnet run` as a background process
- Streams stdout/stderr via `OnLogReceived` event
- Supports graceful shutdown with `StopAsync()`

### Using GitHub Copilot as Agent Client

Integrate with GitHub Copilot's language models:

```csharp
using Microsoft.Copilot.Client;

// Register Copilot client first
builder.Services.AddSingleton<CopilotClient>(sp =>
{
    var client = new CopilotClient();
    // Configure as needed
    return client;
});

// Register orchestration with Copilot
builder.Services.AddOrchestrationWithCopilot(options =>
{
    options.WorkspaceRoot = "./workspaces";
});
```

### Custom Agent Client Registration

Register a custom `IAgentClient` with DI:

```csharp
// Generic method
builder.Services.AddOrchestration<MyCustomAgentClient>(options =>
{
    options.WorkspaceRoot = "./workspaces";
});

// Or replace manually
builder.Services.AddOrchestration();
builder.Services.Replace(
    ServiceDescriptor.Singleton<IAgentClient, MyCustomAgentClient>()
);
```

### Visualizing Agent Flows

Track agent interactions with `AgentCallGraph` (from **Views** package):

```csharp
using ElBruno.AgentsOrchestration.Views;

var callGraph = new AgentCallGraph();

await foreach (var evt in service.Events.Reader.ReadAllAsync())
{
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

Console.WriteLine(callGraph.ToAsciiFlow());
```

**Output:**

```
Orchestrator → Planner: Create execution plan
Orchestrator → Coder: Generate Program.cs
Orchestrator → Designer: Create styles.css
```

---

## Reference: Key Types

### AgentsOrchestration.Abstractions

| Type | Description |
|------|-------------|
| `AgentRole` | Enum: `Orchestrator`, `Planner`, `Coder`, `Designer`, `Fixer` |
| `AgentConfiguration` | Immutable record: role, name, model, instructions, color, icon |
| `IAgentClient` | Interface with single `RunAsync` method — implement for your LLM |
| `AgentFactory` | Creates `AgentSession` instances from the configuration store |
| `AgentSession` | Wraps an `IAgentClient` with a specific agent's configuration |
| `AgentConfigurationStore` | Thread-safe (`ConcurrentDictionary`) agent config management |
| `InstructionLoader` | Loads agent instructions from Markdown files on disk |

### AgentsOrchestration.Orchestration

| Type | Description |
|------|-------------|
| `OrchestrationService` | 6-step pipeline: Plan → Parse → Execute → Verify → Review → Report |
| `IWorkspace` | Interface for workspace creation — implement for your strategy |
| `OrchestrationRequest` | Input record containing the user prompt |
| `OrchestrationResult` | Output record with summary and task results |
| `ExecutionPlan` | Parsed plan with phases and tasks |
| `ExecutionPhase` | A named group of tasks that run in parallel |
| `ExecutionTask` | A single task assigned to an agent with a file scope |
| `TaskResult` | Output from a completed task |
| `OrchestrationEvent` | Base record for 18 event types streamed via `Channel<T>` |

### AgentsOrchestration.Core

| Type | Description |
|------|-------------|
| `TemplateAgentClient` | Deterministic `IAgentClient` — returns template output without an LLM |
| `CopilotAgentClient` | `IAgentClient` implementation using GitHub Copilot SDK |
| `WorkspaceManager` | `IWorkspace` implementation with timestamped directories |
| `AppRunner` | Launches generated apps as background `dotnet run` processes |
| `ConversationManager` | Manages multi-turn conversation sessions with context building |
| `ConversationSession` | Immutable record tracking conversation history and workspace |
| `SessionPersistence` | Saves/loads conversation sessions to/from JSON files |
| `OrchestrationServiceFactory` | Static factory for simplified `OrchestrationService` creation |
| `OrchestrationOptions` | Configuration record for workspace root, fix attempts, custom client/instructions |
| `ServiceCollectionExtensions` | `AddOrchestration()`, `AddOrchestration<TClient>()`, `AddOrchestrationWithCopilot()` |

### AgentsOrchestration.Views

| Type | Description |
|------|-------------|
| `AgentCallGraph` | Tracks and visualizes agent interactions |
| `AgentCallGraphExtensions` | Extension methods for ASCII flow diagram generation |

---

## Next Steps

- Read the [Architecture](architecture.md) document for system design details
- Try the [Getting Started](getting-started.md) guide to run the full dashboard
- Explore agent instructions in `src/AgentsOrchestration.Core/Instructions/`
- Review the [REST API](../README.md#rest-api) for programmatic access
