# ElBruno.AgentsOrchestration — Multi-Agent Orchestration Libraries

A **.NET 10** library suite inspired by [Burke Holland's "Ultralight Orchestration" video](https://www.youtube.com/watch?v=-BhfcPseWFQ&t=586s), implementing the pattern in production-ready C#. Build software automatically using a coordinated team of seven AI agents—**Orchestrator**, **Planner**, **Coder**, **Designer**, **Researcher**, **Fixer**, and **BuildReviewer**—that work together to execute your prompts.

## Overview

The libraries provide a production-ready orchestration engine with:

- **Seven specialized AI agents** that coordinate through a 6-step pipeline (Plan → Parse → Execute → Verify → Review → Report)
- **Research capabilities** — the Researcher agent leverages web search and documentation (Microsoft Learn, Context7) to provide external knowledge
- **Inter-agent communication** — agents can request research and receive enriched context for better results
- **Flow tracing and visualization** — track all agent interactions with Mermaid diagrams and JSON flow data
- **Build validation & self-healing** — automatically detects build errors and invokes the Fixer agent to correct them
- **Build quality analysis** — BuildReviewer analyzes successful builds to provide feedback on performance, security, and best practices
- **Event streaming** — real-time event channel (21 event types) for monitoring orchestration progress
- **Pluggable agent clients** — swap any LLM backend (template, Copilot, OpenAI, Claude, etc.)
- **Workspace isolation** — each run gets a timestamped directory with automatic cleanup
- **.NET Aspire ready** — full support for service discovery, health checks, and distributed tracing
- **OpenTelemetry built-in** — metrics, logs, and traces for production monitoring

## Quick Links

- **📚 [Getting Started](docs/getting-started.md)** — Step-by-step guide with sample prompts
- **🏗️ [Architecture](docs/architecture.md)** — Design patterns and library structure
- **� [Researcher Agent](docs/RESEARCHER_AGENT.md)** — Complete guide to the research agent and inter-agent communication
- **�📖 [Using the Libraries](docs/using-the-libraries.md)** — How to integrate into your own projects
- **🎯 [Samples Overview](docs/samples-overview.md)** — Complete guide to all three samples
- **💡 [Samples](samples/)** — Three ready-to-run examples:
  - **[ConsoleSimple](samples/ConsoleSimple)** — Minimal 7-step demo (weather app)
  - **[ConsoleCompleteChat](samples/ConsoleCompleteChat)** — Full interactive multi-turn chat
  - **[AspireApp](samples/AspireApp)** — Production-grade Blazor dashboard with REST API

---

## Library Packages

### `ElBruno.AgentsOrchestration.Abstractions`

**Zero-dependency** package with core types for building agents:

- Agent roles and configuration
- `IAgentClient` interface
- Agent factory and session management
- Instruction loading utilities

**Use this package when:**

- You''re building your own agent orchestration logic
- You want minimal dependencies
- You''re implementing a custom `IAgentClient` for a different LLM provider

### `ElBruno.AgentsOrchestration.Orchestration`

**Depends on Abstractions** — the orchestration engine and event model:

- 6-step orchestration pipeline (Plan → Parse → Execute → Verify → Review → Report)
- Event streaming via System.Threading.Channels
- 18 event types for monitoring progress (includes BuildReviewStarted/Completed)
- `IWorkspace` abstraction for pluggable storage

**Use this package when:**

- You want the full orchestration pipeline
- You need event streaming to monitor progress
- You''re building a custom UI or integration

### `ElBruno.AgentsOrchestration.Core`

**Depends on Abstractions + Orchestration** — the complete toolkit:

- `TemplateAgentClient` for testing without an LLM
- `WorkspaceManager` with file-based workspace isolation
- `AppRunner` for launching generated applications
- Agent instruction files (Markdown) for all 6 roles
- Integrated with GitHub Copilot SDK and Microsoft Agents AI

**Use this package when:**

- You want the complete out-of-the-box experience
- In tests and sample code
- You''re building a chat interface or dashboard

---

## Installation

### From Local Repository

Clone and reference the projects:

```xml
<ItemGroup>
  <ProjectReference Include="../src/ElBruno.AgentsOrchestration.Core/ElBruno.AgentsOrchestration.Core.csproj" />
</ItemGroup>
```

### NuGet (When Published)

```bash
dotnet add package ElBruno.AgentsOrchestration.Core
```

---

## Quick Start

Here's the minimal code to run an orchestration:

```csharp
using ElBruno.AgentsOrchestration.Core.Orchestration;
using ElBruno.AgentsOrchestration.Orchestration;

// Create service and run (that's it!)
var service = OrchestrationServiceFactory.Create();
var result = await service.RunAsync(
    new OrchestrationRequest("Create a .NET weather console app"),
    CancellationToken.None
);

Console.WriteLine($"✅ Done! Files: {result.WorkspacePath}");
Console.WriteLine($"Summary: {result.Summary}");
```

**With custom configuration:**

```csharp
var service = OrchestrationServiceFactory.Create(options =>
{
    options.WorkspaceRoot = "custom-path";
    options.MaxFixAttempts = 5;
});
```

**For ASP.NET Core / Aspire apps (Dependency Injection):**

```csharp
// In Program.cs
builder.Services.AddOrchestration(options =>
{
    options.WorkspaceRoot = "workspaces";
    options.MaxFixAttempts = 3;
});

// Then inject
public class MyController
{
    private readonly OrchestrationService _service;
    
    public MyController(OrchestrationService service)
    {
        _service = service;
    }
}
```

See [ConsoleSimple](samples/ConsoleSimple) for the complete 6-step walkthrough and [Getting Started](docs/getting-started.md) for detailed usage.

---

## Six-Step Pipeline

| Step | Agent | Description |
|------|-------|-------------|
| 1. **Plan** | 🗺️ Planner | Analyzes prompt, creates implementation plan |
| 2. **Parse** | 🧭 Orchestrator | Parses plan, delegates tasks to agents |
| 3. **Execute** | 💻 Coder / 🎨 Designer | Executes tasks in parallel within phases |
| 4. **Verify** | 🧭 Orchestrator + 🔧 Fixer | Validates build, retries with Fixer on failure |
| 5. **Review** | 📊 BuildReviewer | Analyzes code quality, performance, security |
| 6. **Report** | 🧭 Orchestrator | Provides final summary with review feedback |

---

## Testing

```bash
dotnet test
```

40+ tests cover agents, orchestration, workspace operations, and security.

---

## Samples

### ConsoleSimple — Minimal Demo

```bash
dotnet run --project samples/ConsoleSimple
```

Best for understanding the basic flow. See [ConsoleSimple/README.md](samples/ConsoleSimple/README.md).

### ConsoleCompleteChat — Interactive Multi-Turn

```bash
dotnet run --project samples/ConsoleCompleteChat
```

Full-featured with conversation memory, app launcher, and run command copy. See [ConsoleCompleteChat](samples/ConsoleCompleteChat).

### AspireApp — Production Dashboard

```bash
dotnet run --project samples/AspireApp/AppHost
```

Blazor UI + REST API + OpenTelemetry + Health checks. See [AspireApp](samples/AspireApp).

---

## Documentation

- 📚 [Getting Started](docs/getting-started.md) — Run the samples and try example prompts
- 🏗️ [Architecture](docs/architecture.md) — Design patterns and system structure
- 📖 [Using the Libraries](docs/using-the-libraries.md) — Integrate into your own projects
- ✨ [Smart Instruction Loading](docs/SMART_INSTRUCTION_LOADING.md) — How agent instructions are loaded
- 🔎 [BuildReviewer Implementation](docs/BUILD_REVIEWER_IMPLEMENTATION.md) — Quality analysis phase

---

## License

MIT
