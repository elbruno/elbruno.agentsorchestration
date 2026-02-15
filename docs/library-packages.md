# Library Packages

The ElBruno.AgentsOrchestration suite consists of three NuGet packages, each with a specific purpose. Choose the right package(s) based on your needs.

## `ElBruno.AgentsOrchestration.Abstractions`

**Zero-dependency** package with core types for building agents:

- Agent roles and configuration
- `IAgentClient` interface
- Agent factory and session management
- Instruction loading utilities

**Use this package when:**

- You're building your own agent orchestration logic
- You want minimal dependencies
- You're implementing a custom `IAgentClient` for a different LLM provider

## `ElBruno.AgentsOrchestration.Orchestration`

**Depends on Abstractions** — the orchestration engine and event model:

- 6-step orchestration pipeline (Plan → Parse → Execute → Verify → Review → Report)
- Event streaming via System.Threading.Channels
- 18 event types for monitoring progress (includes BuildReviewStarted/Completed)
- `IWorkspace` abstraction for pluggable storage

**Use this package when:**

- You want the full orchestration pipeline
- You need event streaming to monitor progress
- You're building a custom UI or integration

## `ElBruno.AgentsOrchestration.Core`

**Depends on Abstractions + Orchestration** — the complete toolkit:

- `TemplateAgentClient` for testing without an LLM
- `WorkspaceManager` with file-based workspace isolation
- `AppRunner` for launching generated applications
- Agent instruction files (Markdown) for all 11 agents
- Integrated with GitHub Copilot SDK and Microsoft Agents AI

**Use this package when:**

- You want the complete out-of-the-box experience
- In tests and sample code
- You're building a chat interface or dashboard

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

## Package Dependencies

```
ElBruno.AgentsOrchestration.Abstractions (zero dependencies)
    ↓
ElBruno.AgentsOrchestration.Orchestration (depends on Abstractions)
    ↓
ElBruno.AgentsOrchestration.Core (depends on Abstractions + Orchestration)
    + Microsoft.Agents.AI
    + Microsoft.Agents.AI.GitHub.Copilot
    + Microsoft.Extensions.DependencyInjection.Abstractions
    + Microsoft.Extensions.Options
```
