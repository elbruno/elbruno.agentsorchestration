# Getting Started

This guide walks you through using the **ElBruno.AgentsOrchestration** library and running the sample applications.

> **💡 Tip:** For a complete comparison of all samples, see the [Samples Overview](samples-overview.md).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [.NET Aspire workload](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling) (optional, for the Aspire sample)

Verify your setup:

```bash
dotnet --version         # should print 10.x
```

## Quick Start: Two Ways to Initialize

The library offers two initialization approaches depending on your scenario:

### 1. Static Factory Method (Simplest)

For console apps, scripts, and prototypes:

```csharp
using ElBruno.AgentsOrchestration.Core.Orchestration;
using ElBruno.AgentsOrchestration.Orchestration;

var service = OrchestrationServiceFactory.Create();
var result = await service.RunAsync(new OrchestrationRequest("Your prompt"), CancellationToken.None);
```

**When to use:**

- Quick scripts and demos
- Single-file console applications
- Prototyping and experimentation

### 2. Dependency Injection (Production-Ready)

For ASP.NET Core, Aspire apps, and testable services:

```csharp
using Microsoft.Extensions.DependencyInjection;

// In Program.cs
builder.Services.AddOrchestration(options =>
{
    options.WorkspaceRoot = "custom-path";
    options.MaxFixAttempts = 5;
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

**When to use:**

- ASP.NET Core applications
- .NET Aspire projects
- Unit testing with mocked dependencies
- Production services requiring lifetime management

Both approaches provide the same functionality — choose based on your project's needs.

## 1. Choose Your Sample

### ConsoleSimple — Minimal Demo (Recommended for First Try)

The simplest example showing the basic 6-step flow:

```bash
dotnet run --project samples/ConsoleSimple
```

This demonstrates:

- **Simplified 1-line initialization** using the factory method
- Running a single orchestration request
- Monitoring events in real time
- Viewing generated files
- Getting the run command for the generated app

Perfect for understanding the core concepts without extra complexity. Uses the new simplified API:

```csharp
var service = OrchestrationServiceFactory.Create();
```

### ConsoleCompleteChat — Interactive Multi-Turn

For a full interactive experience with conversation memory:

```bash
dotnet run --project samples/ConsoleCompleteChat
```

This demonstrates:

- Multi-turn conversations with context preservation
- Session management
- App launching from the console
- Copying run commands to run generated apps
- Error handling and recovery

### AspireApp — Full-Featured Dashboard

For a complete, production-grade example with Blazor UI and REST API:

```bash
# Build and launch everything
dotnet run --project samples/AspireApp/AppHost
```

Aspire will launch:

- **Aspire Dashboard** — service health, logs, traces
- **Blazor Web Dashboard** — interactive UI with agent graph visualization
- **REST API** — programmatic orchestration control

Click on the **dashboard** resource in the Aspire dashboard to open the Blazor UI.

## 2. Enter a Prompt

### Console App

The console will prompt you:

```
Enter your request (or press Enter for default):
```

Type a project description or press Enter to use the default (a simple .NET console app).

### Aspire Dashboard

Type a project description into the **chat input** on the left side and press Enter or click Send.

## 3. Watch It Work

### Real-Time Events

Both samples stream events as the agents coordinate:

- **Initialization** — Planner receives the prompt and creates a plan
- **Execution** — Coder and Designer implement the plan in parallel
- **Validation** — Orchestrator runs `dotnet build` to validate generated code
- **Self-Healing** — If build fails, Fixer analyzes and corrects errors (up to 3 retries)
- **Completion** — Final summary and list of generated files

### Console App Output

The console shows emoji-prefixed events:

```
🚀 Started: Create a .NET console application...
📌 Phase 1: Project Setup
🤖 Coder: Create project file
📄 File: project.csproj
✅ Coder completed
🔨 Build succeeded
🏁 Orchestration completed

Files created in: C:\Temp\orchestration-abc123
Generated files:
  - project.csproj
  - Program.cs
```

### Aspire Dashboard

Monitor progress through:

- **Agent Graph** — 5 nodes with pulsing animations as agents activate
- **Activity Feed** — scrollable timestamped log
- **Workspace Viewer** — expanding file tree of generated output
- **App Controls** — Launch or stop the generated application

### Build Validation & Self-Healing

After the Coder generates files, the Orchestrator automatically runs `dotnet build` to validate the output. If the build fails:

1. The **Fixer** agent (🔧) receives the build errors and failing source code
2. It produces corrected files with minimal changes
3. The build is retried automatically (up to 3 attempts by default)
4. All fix attempts appear in the Activity Feed in real time

### Launching Generated Apps

Once a build succeeds, a **🚀 Launch App** button becomes available. Click it to run the generated app as a background process. Stdout/stderr output streams directly into the Activity Feed. Click **⏹ Stop App** to shut it down.

### Conversation Memory

The dashboard remembers your conversation. After the first run completes, send a follow-up message to modify the generated project — the agents receive the full conversation context so they can make incremental changes without regenerating everything.

## 3. Sample Prompts

These prompts demonstrate the range of projects the agents can tackle. Paste any of them into the prompt input to try them out.

### Blazor Web App

```
Create a Blazor Server app that manages cooking recipes. Include a Recipe model
with name, ingredients, and instructions. Add pages to list all recipes, view
details, and create or edit a recipe. Use Bootstrap for styling.
```

**What to expect:** The Planner breaks this into phases (data model → service → pages → styling). The Coder creates `.cs` and `.razor` files while the Designer produces CSS and layout tweaks. Output appears in the Workspace Viewer under a timestamped directory.

### Static Landing Page

```
Build a responsive landing page for a SaaS product called "TaskFlow". Include a
hero section with a call-to-action button, a features grid with three cards, a
pricing table with Free, Pro, and Enterprise tiers, and a footer with social links.
Use plain HTML and CSS only.
```

**What to expect:** Mostly Designer-driven. The Coder generates the HTML structure and the Designer creates the CSS with responsive breakpoints.

### REST API

```
Create a minimal ASP.NET Core Web API for a to-do list. Include endpoints to
list, create, update, and delete to-do items. Store items in-memory using a
ConcurrentDictionary. Return JSON with proper HTTP status codes.
```

**What to expect:** Primarily Coder output — a `Program.cs` with mapped endpoints and a `TodoItem` record. The Designer may add an `index.html` with basic styling for API documentation.

### Console Utility

```
Build a .NET console app that reads a CSV file, groups rows by a user-specified
column, and prints a summary table with counts per group. Use Spectre.Console for
the table output.
```

**What to expect:** A single-phase, Coder-focused run producing a `Program.cs` and supporting model classes.

### Multi-Turn: Build Then Iterate

These prompts show how conversation memory lets you refine a project across multiple turns.

**Turn 1 — scaffold:**

```
Create a Blazor Server dashboard that shows real-time weather for three cities
(London, Tokyo, New York). Use a WeatherService that returns random temperatures,
a card per city, and auto-refresh every 5 seconds with a Timer.
```

**Turn 2 — add a feature:**

```
Add a search box at the top so users can add more cities. Persist the city list
in localStorage via JS interop.
```

**Turn 3 — restyle:**

```
Switch to a dark theme. Use CSS variables for all colors and add subtle hover
animations on the city cards.
```

**What to expect:** Each turn builds on the previous workspace. The agents see the full conversation and make targeted changes rather than regenerating from scratch.

### Full-Stack: API + UI + Database Schema

```
Build a .NET solution with two projects. First, an ASP.NET Core minimal API
("InventoryApi") with CRUD endpoints for products (Id, Name, Sku, Price,
Quantity). Use Entity Framework Core with an in-memory database. Second, a
Blazor Server app ("InventoryDashboard") that consumes the API with HttpClient,
displays products in a searchable DataGrid, and has forms for add/edit. Share
a "Contracts" class library between both projects for DTOs.
```

**What to expect:** The Planner creates a multi-phase plan spanning three projects. The Coder generates the API, shared contracts, and Blazor client. The Designer styles the DataGrid and forms.

## 4. Customize Agents

Navigate to **Settings** (via the sidebar) to change each agent's configuration:

- **Model** — switch between LLMs (e.g., `gpt-5.3-codex`, `claude-sonnet-4.5`)
- **Instructions** — edit the system prompt that guides each agent's behavior
- **Reset** — revert an agent's instructions to the defaults from `Core/Instructions/*.md`

Changes take effect on the next orchestration run.

## 5. Inspect Generated Output

Each run creates a timestamped workspace directory under the configured root path (default: `workspaces/`). You can browse files in the **Workspace Viewer** or open them directly from disk:

```
workspaces/
  20260214153012-create-a-blazor-server-app/
    src/Generated/App.cs
    wwwroot/generated.css
```

## Next Steps

- Follow [Using the Libraries](using-the-libraries.md) to learn how to use the NuGet packages in your own projects
- Read the full [Architecture](architecture.md) document for system design details
- Review the [README](../README.md) for project structure and API reference
- Explore the orchestration plan in [docs/plans/orchestration-plan.md](plans/orchestration-plan.md)
- Look at the agent instructions in `src/AgentsOrchestration.Core/Instructions/` to understand how each agent is guided
- Use the [REST API](../README.md#rest-api) endpoints for programmatic access to agent configuration and orchestration
