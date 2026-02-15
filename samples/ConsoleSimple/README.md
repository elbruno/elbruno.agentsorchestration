# ConsoleSimple — Step-by-Step Demo

This sample demonstrates the **minimal setup** needed to use the **ElBruno.AgentsOrchestration** libraries. It's designed to show the basic flow in the simplest way possible.

## What It Does

Creates a working weather console app using a single orchestration request. The app displays randomly generated weather for three cities (London, Tokyo, New York) with temperature and weather emoji.

**Important:** This sample uses `TemplateAgentClient`, which returns pre-written, valid C# code for demonstration purposes. In production, you would replace it with a real LLM-based agent client (OpenAI, Azure OpenAI, Ollama, etc.).

## How to Run

```bash
dotnet run --project samples/ConsoleSimple
```

## Code Walkthrough

The [Program.cs](Program.cs) file shows 7 simple steps:

1. **Initialize components** — Create the configuration store, agent client, factory, and workspace manager
2. **Create orchestration service** — Combine components into the orchestration service
3. **Define the prompt** — Specify what you want the agents to build
4. **Listen to events** (optional) — Monitor progress in real-time
5. **Run orchestration** — Execute the request with `RunAsync()`
6. **View generated files** — List the output files
7. **Get run command** — Copy the command to run the generated app

## Key Concepts

### Components

```csharp
var store = new AgentConfigurationStore(InstructionLoader.LoadInstructions());
var client = new TemplateAgentClient();
var factory = new AgentFactory(store, client);
var workspaceManager = new WorkspaceManager(workspaceRoot);
```

- **AgentConfigurationStore** — holds settings for each agent (model, instructions)
- **TemplateAgentClient** — the agent client implementation (returns template responses; swap for real LLM)
- **AgentFactory** — creates agent instances on demand
- **WorkspaceManager** — manages isolated workspace directories for each run

### Orchestration

```csharp
var service = new OrchestrationService(factory, workspaceManager);
var result = await service.RunAsync(
    new OrchestrationRequest(prompt),
    CancellationToken.None
);
```

The orchestration service:

- Runs the 6-step pipeline (Plan → Parse → Execute → Verify → Review → Report)
- Activates agents (Planner, Orchestrator, Coder, Designer, Fixer, BuildReviewer) as needed
- Validates the build and retries with the Fixer if errors occur
- Returns a result with summary and task results
- Stores files in the workspace manager's WorkspacePath

### Events

```csharp
await foreach (var evt in service.Events.Reader.ReadAllAsync())
{
    // Process event (e.g., log to console)
}
```

The event channel streams 18 event types in real-time, letting you monitor orchestration progress without blocking the main execution.

## Template vs Real Code Generation

This sample uses `TemplateAgentClient`, which returns **pre-written valid C# code** for common scenarios like weather apps, to-do APIs, and Blazor apps. This approach ensures:

✅ The demo always works without requiring an LLM API key  
✅ The generated code compiles and runs successfully  
✅ You can understand the library flow without LLM complexity  

**When you're ready for production:**

Replace `TemplateAgentClient` with your own `IAgentClient` implementation that calls a real LLM:

```csharp
public sealed class OpenAiAgentClient : IAgentClient
{
    private readonly HttpClient _httpClient;
    
    public async Task<string> RunAsync(
        AgentRole role, 
        string prompt, 
        string workspacePath, 
        CancellationToken ct)
    {
        // Call OpenAI, Azure OpenAI, Ollama, etc.
        var response = await CallYourLlmAsync(role, prompt, ct);
        return response;
    }
}

// Register it in DI
builder.Services.AddSingleton<IAgentClient, OpenAiAgentClient>();
```

With a real LLM, the agents can generate code for **any** prompt, not just pre-defined scenarios.

## Next Steps

- Try [ConsoleCompleteChat](../ConsoleCompleteChat) for a full interactive experience with multi-turn conversations
- Explore [AspireApp](../AspireApp) for a production-ready Blazor dashboard with REST API
- Read [Using the Libraries](../../docs/using-the-libraries.md) to integrate into your own projects
- Replace `TemplateAgentClient` with a real LLM client to generate code from any prompt
