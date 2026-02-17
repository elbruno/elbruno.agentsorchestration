# Samples Overview

This repository includes six samples that demonstrate the **ElBruno.AgentsOrchestration** libraries at different levels of complexity. Choose the one that matches your learning goals.

## Quick Comparison

| Sample | Complexity | Best For | Key Features |
|--------|-----------|----------|--------------|
| **[ConsoleSimple](../samples/ConsoleSimple)** | ⭐ Minimal | Learning the basics | Factory method setup, event streaming, single orchestration |
| **[ConsoleCompleteChat](../samples/ConsoleCompleteChat)** | ⭐⭐ Intermediate | Interactive use | Multi-turn chat, session management, app launcher |
| **[ConsoleFlowTraces](../samples/ConsoleFlowTraces)** | ⭐⭐ Intermediate | Debugging & visualization | Agent flow tracking, verbose event logging, ASCII diagrams |
| **[AddAndListCustomAgents](../samples/AddAndListCustomAgents)** | ⭐⭐ Intermediate | Dynamic agent loading | Load agents from Awesome Copilot, agent management |
| **[ConsoleWinFormsGenerator](../samples/ConsoleWinFormsGenerator)** | ⭐⭐⭐ Advanced | Generating WinForms apps | Dynamic agents in action, application generation |
| **[AspireApp](../samples/AspireApp)** | ⭐⭐⭐ Advanced | Production patterns | Blazor UI, REST API, SignalR, health checks, tracing |

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

## ConsoleFlowTraces — Agent Flow Visualization

**Location:** `samples/ConsoleFlowTraces/`

**Purpose:** Demonstrate how to track, visualize, and debug agent interactions with detailed event logging and ASCII flow diagrams.

**What it does:**

- **Verbose event logging** — Displays detailed information for all orchestration events with color coding
- **Agent flow tracking** — Records agent calls and builds an interaction graph
- **ASCII flow diagram** — Visualizes the agent call sequence at the end
- **Research and review tracking** — Shows when specialist agents (Researcher, BuildReviewer) are activated
- **Performance metrics** — Displays timing information and event counts

**When to use this:**

- You want to understand how agents coordinate
- You're debugging orchestration issues
- You need to visualize the agent interaction flow
- You want detailed logs of every step in the pipeline

**Run it:**

```bash
dotnet run --project samples/ConsoleFlowTraces
```

**Key features:**

- **Color-coded events** — Different colors for different event types (phases, agents, files, builds, errors)
- **Detailed timestamps** — Every event is timestamped with `[HH:mm:ss]` format
- **Agent call tracking** — Uses `AgentCallGraph` to record all agent interactions
- **Flow visualization** — Produces an ASCII diagram showing agent coordination patterns
- **Research event tracking** — Tracks `ResearchRequestedEvent` and `ResearchCompletedEvent` for Researcher agent
- **Build review tracking** — Monitors `BuildReviewStartedEvent` and `BuildReviewCompletedEvent`
- **Fix attempt tracking** — Shows detailed information for each self-healing attempt

**Key files:**

- [Program.cs](../samples/ConsoleFlowTraces/Program.cs) — Full implementation with verbose event handling
- Uses `AgentCallGraph` from the Views library
- Uses `OrchestrationServiceFactory.Create()` for simplified setup

**Output example:**

```
[19:30:15] 📌 PHASE START      : Phase 1: PROJECT SETUP
[19:30:15] 🤖 AGENT ACTIVE     : Planner           | Task: Create execution plan
[19:30:15] 📝 PLAN UPDATE      : Planner updated execution plan
[19:30:16] 🤖 AGENT ACTIVE     : Coder             | Task: Create Program.cs
[19:30:16] 📄 FILE CREATED     : Program.cs
[19:30:17] 🔨 BUILD SUCCESS    : Project compiled successfully.
[19:30:17] 🏁 FINISHED         : All tasks completed.

📊 Agent Interaction Flow:
Orchestrator → Planner: Create execution plan
Orchestrator → Coder: Create Program.cs
Orchestrator → Coder: Create project.csproj
Orchestrator → Designer: Create styles.css
```

This sample is perfect for learning how the orchestration engine works under the hood.

---

## AddAndListCustomAgents — Dynamic Agent Loading

**Location:** `samples/AddAndListCustomAgents/`

**Purpose:** Demonstrate how to dynamically load and register custom agent definitions from the Awesome Copilot Repository.

**What it does:**

- Loads the 11 built-in static agents
- Downloads and registers **WinForms Expert** from Awesome Copilot Repository
- Downloads and registers **Security Reviewer** from Awesome Copilot Repository
- Lists all available agents (static and dynamic)
- Demonstrates agent management and querying

**When to use this:**

- You need specialized agents for specific domains (WinForms, security, etc.)
- You want to extend the orchestration system without modifying core code
- You're building a plugin architecture with community agents
- You need to load custom agents from your organization

**Run it:**

```bash
dotnet run --project samples/AddAndListCustomAgents
```

**Code highlight:**

```csharp
// Initialize agent manager
var staticStore = new AgentConfigurationStore();
var loader = new AwesomeCopilotAgentLoader();
var manager = new DynamicAgentManager(staticStore, loader);

// Load agents from Awesome Copilot Repository
var winFormsAgent = await manager.LoadAndRegisterAgentAsync("WinFormsExpert");
var securityAgent = await manager.LoadAndRegisterAgentAsync("se-security-reviewer");

// Query agents
var allAgents = manager.GetAllAgents();
Console.WriteLine($"Total: {allAgents.Count} agents");
```

**Key features:**

- HTTP client with caching for downloaded agents
- YAML front matter parsing for agent metadata
- Agent lifecycle management (load, unload, query)
- Support for multiple sources (GitHub, URL, local file)

**Output example:**

```
🤖 Dynamic Agent Loading Sample
================================

✅ Loaded 11 static agents

📦 Loading WinForms Expert agent...
✅ Loaded: WinForms Expert
   Icon: 👨‍💼
   Instructions preview: # WinForms Development Guidelines...

📦 Loading Security Reviewer agent...
✅ Loaded: SE: Security
   Icon: 🔒

📋 All available agents:
   ⚙️ 📊 BuildReviewer
   ⚙️ 💻 Coder
   ...
   🔌 🔒 SE: Security
   🔌 👨‍💼 WinForms Expert

✅ Total: 13 (11 static, 2 dynamic)
```

See the [Dynamic Agents documentation](../docs/DYNAMIC_AGENTS.md) for complete usage guide.

---

## ConsoleWinFormsGenerator — Generate WinForms Applications

**Location:** `samples/ConsoleWinFormsGenerator/`

**Purpose:** Demonstrate how to use dynamic Agent loading to generate a complete Windows Forms application with professional structure and best practices.

**What it does:**

- Loads the 11 built-in core agents
- Dynamically loads the **WinForms Expert** agent from Awesome Copilot Repository
- Generates a complete **Todo Manager WinForms application** with:
  - Professional UI with proper form design
  - MVVM pattern with data models
  - CRUD operations (Create, Read, Update, Delete)
  - JSON-based data persistence
  - Input validation and error handling
  - WinForms best practices and patterns
- Validates the generated code and auto-fixes any build errors
- Provides build review feedback on quality and best practices

**When to use this:**

- You want to generate complete WinForms applications automatically
- You need to see dynamic agents in action generating real code
- You're learning application generation patterns
- You want to understand how specialized agents improve code quality
- You need a template for WinForms project generation

**Run it:**

```bash
dotnet run --project samples/ConsoleWinFormsGenerator
```

**Code highlight:**

```csharp
// Initialize core agents
var staticStore = new AgentConfigurationStore();

// Load WinForms Expert dynamically
var loader = new AwesomeCopilotAgentLoader();
var manager = new DynamicAgentManager(staticStore, loader);
var winFormsAgent = await manager.LoadAndRegisterAgentAsync("WinFormsExpert");

// Create orchestration service with dynamic agents
var service = OrchestrationServiceFactory.Create();

// Generate application
var result = await service.RunAsync(
    new OrchestrationRequest("Create a professional WinForms Todo Manager application..."),
    CancellationToken.None
);
```

**Key features:**

- **Dynamic Agent Integration** — Specialized WinForms expert improves code quality
- **Full Application Generation** — Complete, buildable WinForms project with all files
- **Multi-Agent Coordination** — Planner, Coder, Designer, Fixer, and BuildReviewer work together
- **Auto-Repair** — The Fixer agent automatically corrects any build errors
- **Quality Analysis** — BuildReviewer provides feedback on code quality and patterns
- **Production-Ready** — Generated code follows .NET best practices and WinForms patterns

**Output example:**

```
🪟 WinForms Application Generator with Dynamic Agent
====================================================

✅ Loaded 11 core agents

📦 Loading WinForms Expert agent...
✅ Loaded: WinForms Expert (👨‍💼)

🔨 Starting orchestration to generate WinForms application...

[Real-time orchestration events streaming...]

✨ Orchestration completed!

📁 Workspace: D:\...\samples\workspaces\20260216143022-create-a-professional-winforms/
📊 Status: CompletedSuccessfully

📄 Summary:
✅ Successfully generated a complete WinForms Todo Manager application
   - MainForm.cs with ListView control
   - TaskModel.cs data model
   - TaskRepository.cs with JSON persistence
   - Input validation and error handling
   - Proper MVVM pattern implementation

🎉 Your WinForms application is ready!
```

**Generated Project Structure:**

```
workspace/
├── Program.cs               # Application entry point
├── MainForm.cs              # Main UI form
├── MainForm.Designer.cs     # Designer-generated UI code  
├── TaskModel.cs             # Data model
├── TaskRepository.cs        # Data persistence
├── project.csproj           # Project configuration
└── bin/                     # Compiled output
```

Perfect for understanding how the orchestration system can generate complete applications with specialized agents!

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

### I want to visualize and debug agent interactions

→ Try **ConsoleFlowTraces**. It shows detailed event logging and agent flow diagrams.

### I want to understand dynamic agent loading

→ Explore **AddAndListCustomAgents**. It shows how to load specialized agents from the community.

### I want to see agents generating applications

→ Try **ConsoleWinFormsGenerator**. It demonstrates agents working together to generate a complete WinForms application using dynamic agent expertise.

### I'm building a web app or deploying to production

→ Use **AspireApp** as your template. It includes everything: UI, API, health checks, tracing.

### I'm integrating into my own project

→ Read [Using the Libraries](using-the-libraries.md) and start with **ConsoleSimple** to understand the core components.

---

## Common Patterns Across All Samples

All six samples follow the same core initialization pattern:

```csharp
// Option 1: Simplified factory method (ConsoleSimple, ConsoleFlowTraces)
var service = OrchestrationServiceFactory.Create();

// Option 2: Manual setup (ConsoleCompleteChat)
var store = new AgentConfigurationStore(InstructionLoader.LoadInstructions());
var client = new TemplateAgentClient();
var factory = new AgentFactory(store, client);
var workspace = new WorkspaceManager(rootPath);
var service = new OrchestrationService(factory, workspace);

// Option 3: Dependency injection (AspireApp)
builder.Services.AddOrchestration(options =>
{
    options.WorkspaceRoot = "./workspaces";
});
```

The orchestration flow is identical across all samples:

```csharp
// Run an orchestration
var result = await service.RunAsync(
    new OrchestrationRequest(prompt),
    CancellationToken.None
);
```

The only differences are:

- **ConsoleSimple** — runs once and exits, minimal event logging
- **ConsoleCompleteChat** — wraps in a loop with session management and conversation memory
- **ConsoleFlowTraces** — verbose event logging with agent flow visualization
- **AddAndListCustomAgents** — demonstrates dynamic agent loading from repository
- **ConsoleWinFormsGenerator** — uses dynamic agents to generate complete applications
- **AspireApp** — runs as a web service with Blazor UI and SignalR streaming

---

## Next Steps

- Run each sample in order to see the progression from simple to complex
- Read the [Getting Started](getting-started.md) guide for detailed walkthroughs
- Review the [Architecture](architecture.md) document for design patterns
- Follow [Using the Libraries](using-the-libraries.md) to integrate into your own projects
