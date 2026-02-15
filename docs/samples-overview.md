# Samples Overview

This repository includes three samples that demonstrate the **ElBruno.AgentsOrchestration** libraries at different levels of complexity. Choose the one that matches your learning goals.

## Quick Comparison

| Sample | Complexity | Best For | Key Features |
|--------|-----------|----------|--------------|
| **[ConsoleSimple](../samples/ConsoleSimple)** | ⭐ Minimal | Learning the basics | Simplified 1-line setup, event streaming, weather app |
| **[ConsoleCompleteChat](../samples/ConsoleCompleteChat)** | ⭐⭐ Intermediate | Interactive use | Multi-turn chat, session management, app launcher |
| **[AspireApp](../samples/AspireApp)** | ⭐⭐⭐ Advanced | Production patterns | Blazor UI, REST API, health checks, tracing |

---

## ConsoleSimple — Minimal Demo

**Location:** `samples/ConsoleSimple/`

**Purpose:** Show the absolute minimum code needed to run an orchestration using the simplified API.

**What it does:**

- **1-line initialization** using `OrchestrationServiceFactory.Create()`
- Runs a single orchestration request to build a weather console app
- Displays events in real-time with emoji prefixes
- Lists generated files
- Shows the `dotnet run` command to execute the generated app

**When to use this:**

- You're new to the library
- You want to understand the basic flow
- You're looking for copy-paste starter code

**Run it:**

```bash
dotnet run --project samples/ConsoleSimple
```

**Code highlight:**

```csharp
// That's it - one line!
var service = OrchestrationServiceFactory.Create();
var result = await service.RunAsync(new OrchestrationRequest(prompt), CancellationToken.None);
```

**Key files:**

- [Program.cs](../samples/ConsoleSimple/Program.cs) — 6-step walkthrough with comments
- [README.md](../samples/ConsoleSimple/README.md) — Explanation of each step

**Output:** Creates a timestamped workspace with a weather app (`Program.cs`, `project.csproj`).

---

## ConsoleCompleteChat — Interactive Multi-Turn

**Location:** `samples/ConsoleCompleteChat/`

**Purpose:** Demonstrate a full-featured interactive console with conversation memory and session management.

**What it does:**

- Multi-turn conversations — add features, modify code, iterate on designs
- Session management — track multiple workspaces and conversation history
- App launcher — run generated apps as background processes
- Run command display — copy the `dotnet run` command for the generated app
- Error recovery — gracefully handles failures and allows retries

**When to use this:**

- You're building a chat-based interface
- You need conversation context preservation
- You want to see how to manage multiple sessions

**Run it:**

```bash
dotnet run --project samples/ConsoleCompleteChat
```

**Key features:**

- **Main menu** — start new orchestration, list sessions, exit
- **Conversation menu** — add features, run app, view files, show run command, start new, exit
- **Conversation memory** — uses `ConversationManager` to build context prompts from history
- **App runner** — uses `AppRunner` to launch and monitor generated apps
- **Event streaming** — displays real-time progress with 18 event types

**Key files:**

- [Program.cs](../samples/ConsoleCompleteChat/Program.cs) — Full implementation
- `AgentFactory`, `WorkspaceManager`, `ConversationManager`, `AppRunner` — from Core library

**Output:** Creates workspaces and tracks conversations across multiple runs.

---

## AspireApp — Production Dashboard

**Location:** `samples/AspireApp/`

**Purpose:** Showcase a production-grade architecture with Blazor UI, REST API, health checks, and distributed tracing.

**What it does:**

- **Blazor Web Dashboard** — Interactive UI with agent graph, activity feed, workspace viewer
- **REST API** — Programmatic access to agent configuration and orchestration
- **SignalR Hub** — Real-time event streaming from server to browser
- **Health checks** — Liveness and readiness probes for Kubernetes/cloud deployments
- **OpenTelemetry** — Logs, metrics, and traces exported to OTLP and Azure Monitor
- **.NET Aspire** — Service discovery, orchestration, and the Aspire dashboard

**When to use this:**

- You're building a web application
- You need a REST API for orchestration
- You're deploying to production (cloud, Kubernetes, etc.)
- You want to explore .NET Aspire patterns

**Run it:**

```bash
dotnet run --project samples/AspireApp/AppHost
```

**Services launched:**

1. **Aspire Dashboard** — `http://localhost:15xxx` — service health, logs, traces
2. **Blazor Web App** — `https://localhost:7xxx` — interactive UI
3. **REST API** — `https://localhost:7xxx` — programmatic access

**Key features:**

### Blazor Web Dashboard

- **Agent Graph** — Visual representation of 6 agents with status animations
- **Activity Feed** — Scrollable real-time event log
- **Workspace Viewer** — File tree of generated output
- **App Controls** — Launch/stop generated apps, stream logs
- **Settings Page** — Edit agent configurations (model, instructions)
- **Multi-turn conversations** — Preserve context across orchestration runs

### REST API

- `GET /agents` — List all agent configurations
- `GET /agents/{role}` — Get a specific agent's configuration
- `PUT /agents/{role}` — Update agent configuration
- `POST /agents/{role}/reset` — Reset agent instructions to defaults
- `POST /orchestration/run` — Start a new orchestration
- `GET /orchestration/status/{id}` — Check orchestration status
- `POST /orchestration/cancel/{id}` — Cancel a running orchestration

### Infrastructure

- **ServiceDefaults** — Shared OpenTelemetry, health checks, resilience configuration
- **AppHost** — Aspire orchestrator that wires services together
- **SignalR Hub** — Streams events from `OrchestrationService.Events` to connected clients

**Key files:**

- `Web/Components/Pages/Home.razor` — Main dashboard page
- `Web/Components/Pages/Settings.razor` — Agent configuration editor
- `Web/Hubs/OrchestrationHub.cs` — SignalR hub for real-time events
- `Api/Program.cs` — REST API endpoints
- `AppHost/Program.cs` — Aspire orchestration
- `ServiceDefaults/Extensions.cs` — Shared OpenTelemetry and health check setup

**Output:** Full web application with monitoring, observability, and production-ready patterns.

---

## Which Sample Should I Start With?

### I want to understand the basics

→ Start with **ConsoleSimple**. It's the shortest path from zero to orchestration.

### I'm building a chat interface

→ Use **ConsoleCompleteChat** as a reference. It shows conversation memory and session management.

### I'm building a web app or deploying to production

→ Use **AspireApp** as your template. It includes everything: UI, API, health checks, tracing.

### I'm integrating into my own project

→ Read [Using the Libraries](using-the-libraries.md) and start with **ConsoleSimple** to understand the core components.

---

## Common Patterns Across All Samples

All three samples follow the same core initialization pattern:

```csharp
// 1. Load agent configurations
var store = new AgentConfigurationStore(InstructionLoader.LoadInstructions());

// 2. Create the agent client (LLM abstraction)
var client = new TemplateAgentClient();

// 3. Create the agent factory
var factory = new AgentFactory(store, client);

// 4. Create the workspace manager
var workspace = new WorkspaceManager(rootPath);

// 5. Create the orchestration service
var service = new OrchestrationService(factory, workspace);

// 6. Run an orchestration
var result = await service.RunAsync(
    new OrchestrationRequest(prompt),
    CancellationToken.None
);
```

The only differences are:

- **ConsoleSimple** — runs once and exits
- **ConsoleCompleteChat** — wraps in a loop with session management
- **AspireApp** — runs as a web service with dependency injection

---

## Next Steps

- Run each sample in order to see the progression from simple to complex
- Read the [Getting Started](getting-started.md) guide for detailed walkthroughs
- Review the [Architecture](architecture.md) document for design patterns
- Follow [Using the Libraries](using-the-libraries.md) to integrate into your own projects
