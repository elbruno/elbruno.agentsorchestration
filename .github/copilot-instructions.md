# Copilot Instructions — Multi-Agent Orchestration Libraries

## Architecture

This is a .NET 10 (`net10.0`) solution with three library projects, sample applications, and tests, using the `slnx` format:

- **ElBruno.AgentsOrchestration.Abstractions** — Zero-dependency package with core types: agent roles, configuration, IAgentClient interface.
- **ElBruno.AgentsOrchestration.Orchestration** — Orchestration engine with 6-step pipeline and event streaming.
- **ElBruno.AgentsOrchestration.Core** — Complete toolkit with TemplateAgentClient, WorkspaceManager, and agent instructions.
- **samples/ElBruno.AgentsOrchestration.Console** — Interactive console sample with multi-turn conversations.
- **samples/ElBruno.AgentsOrchestration.AspireApp** — Production-grade Aspire app with Blazor dashboard, REST API, health checks, and distributed tracing.
- **tests/AgentsOrchestration.Core.Tests** — xUnit integration and unit tests.

The system implements Burke Holland's "Ultralight Orchestration" pattern with 11 specialized AI agents: six core agents (Orchestrator, Planner, Coder, Designer, Fixer, BuildReviewer) coordinate through a 6-step pipeline (Plan → Parse → Execute → Verify → Review → Report), plus five specialist agents (Researcher, SecurityExpert, TestingExpert, DocumentationExpert, SoftwareArchitect) for extended capabilities.

## Key Patterns

### Agent abstraction via `IAgentClient`

All LLM calls go through `IAgentClient.RunAsync(AgentRole, prompt, workspacePath, CancellationToken)`. The current implementation is `TemplateAgentClient` (returns deterministic template strings). When adding a real LLM provider, implement `IAgentClient` and register it in `Program.cs`. Do not add LLM logic directly to `AgentFactory` or `OrchestrationService`.

### Immutable records for data, mutable classes for UI state

Domain models in `Core/Orchestration/Models.cs` and `Core/Orchestration/OrchestrationEvents.cs` are `sealed record` types. `AgentConfiguration` is also a `sealed record` — use `with` expressions to update. Mutable classes (`AgentNodeState`, `ActivityItem`) are only in `Web/Models/DashboardModels.cs` for Blazor binding.

### Event streaming via `Channel<OrchestrationEvent>`

`OrchestrationService.Events` is an unbounded `Channel<OrchestrationEvent>`. The hub and Home.razor both consume it with `ReadAllAsync()`. Events use a sealed record hierarchy rooted at `OrchestrationEvent(DateTimeOffset Timestamp)`. When adding new event types, add a new sealed record inheriting from `OrchestrationEvent` and handle it in `OrchestrationHub.ToMessage()` and `Home.razor.ApplyEvent()`.

### Service lifetimes

- **Singleton**: `AgentConfigurationStore` (thread-safe via `ConcurrentDictionary`), `IAgentClient`, `AgentFactory`
- **Scoped**: `WorkspaceManager`, `OrchestrationService` — created per orchestration run (the hub creates its own scope via `IServiceScopeFactory`)

### Workspace isolation

Each orchestration run creates a timestamped directory under `Workspace:RootPath` (default `../../workspaces`). All agent file operations target this shared workspace path. The `workspaces/` directory is gitignored.

### Agent instructions

Markdown files in `src/ElBruno.AgentsOrchestration.Core/Instructions/{role}.md` are loaded via `InstructionLoader.LoadInstructions()` which uses a smart fallback chain:

1. Environment variable `AGENT_INSTRUCTIONS_PATH` (if set)
2. Development paths (relative to build output)
3. Built-in defaults (always works)

When adding a new agent role:

1. Add the enum value to `ElBruno.AgentsOrchestration.Abstractions/Agents/AgentRole.cs`
2. Create the `.md` file in `src/ElBruno.AgentsOrchestration.Core/Instructions/`
3. Update `InstructionLoader.GetDefaultInstructions()` with built-in fallback
4. Update `AgentConfigurationStore.CreateDefaultConfigurations()`

## Documentation & Plans

### Repository root

Keep the root clean. Only these files belong here: `README.md`, `LICENSE`, `AgentsOrchestration.slnx`, `.gitignore` / `.gitattributes`, and `.github/`. All extended documentation goes in `docs/`.

### Documentation rules

- `README.md` stays in the root.
- Any doc that is **not** the README or LICENSE must go in `docs/`.
- When adding new documentation, create it under `docs/`, not in the root.

### Plans

- All plans are saved in `docs/plans/`.
- Plan files **must** use the naming format: `plan_YYMMDD_HHmm.md` where `YYMMDD` is the 2-digit year, month, day and `HHmm` is the 24-hour time.
- Example: `plan_260214_1930.md` → 2026-02-14 at 19:30.
- When a plan is implemented, update the roadmap file in `docs/plans/` (e.g. `roadmap_*.md`) in the same change to record completion status, date, and plan link.

## Build & Test

```bash
dotnet build AgentsOrchestration.slnx                                         # Build entire solution
dotnet test                                                                    # Run all tests (40+ tests)
dotnet run --project samples/ElBruno.AgentsOrchestration.Console              # Interactive console sample
dotnet run --project samples/ElBruno.AgentsOrchestration.AspireApp/AppHost    # Aspire dashboard with API + UI
```

Tests create temp directories and clean up in `finally` blocks — follow this pattern for new tests.

## Conventions

- Target `net10.0` with `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`
- Use `sealed` on all concrete classes and records
- Blazor components live in `Web/Components/` (shared) and `Web/Components/Pages/` (routable). Shared components use parameters (`[Parameter]`); pages inject services directly
- SignalR hub at `/hubs/orchestration` — maps events to anonymous `{ Type, Message, Timestamp }` objects
- The orchestration plan in `BuildPlanAsync()` currently returns a hardcoded plan — this is the primary extension point for dynamic plan parsing from Planner output
- Bootstrap is the CSS framework (loaded from `wwwroot/lib/bootstrap/`)
