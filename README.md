# Agents Orchestration — Multi-Agent Orchestration Libraries

[![NuGet](https://img.shields.io/nuget/v/ElBruno.AgentsOrchestration.Core.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/ElBruno.AgentsOrchestration.Core/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ElBruno.AgentsOrchestration.Core.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/ElBruno.AgentsOrchestration.Core/)
[![Build Status](https://github.com/elbruno/elbruno.agentsorchestration/actions/workflows/publish.yml/badge.svg)](https://github.com/elbruno/elbruno.agentsorchestration/actions/workflows/publish.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](LICENSE)
[![GitHub stars](https://img.shields.io/github/stars/elbruno/elbruno.agentsorchestration?style=social)](https://github.com/elbruno/elbruno.agentsorchestration)
[![Twitter Follow](https://img.shields.io/twitter/follow/elbruno?style=social)](https://twitter.com/elbruno)

A **.NET 10** library suite inspired by [Burke Holland's "Ultralight Orchestration" video](https://www.youtube.com/watch?v=-BhfcPseWFQ), implementing the pattern in production-ready C#, using **Microsoft Agent Framework** and **GitHub Copilot SDK** to create a powerful multi-agent orchestration system.

Build software automatically using a coordinated team of **11 specialized AI agents**—including **Orchestrator**, **Planner**, **Coder**, **Designer**, **Researcher**, **Fixer**, **BuildReviewer**, **SecurityExpert**, **TestingExpert**, **DocumentationExpert**, and **SoftwareArchitect**—that work together to execute your prompts.

## Overview

The libraries provide a production-ready orchestration engine with:

- **11 specialized AI agents powered by GitHub Copilot SDK** — 6 core agents for the orchestration pipeline and 5 specialist agents for extended capabilities (security, testing, documentation, architecture, research)
- **GitHub Copilot integration** — all agents leverage the GitHub Copilot SDK for intelligent code generation, analysis, and planning
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
- **📦 [Library Packages](docs/library-packages.md)** — Core abstractions, orchestration engine, and complete toolkit
- **🤖 [All Agents](docs/agents.md)** — Complete guide to all 11 agents and their roles
- **🔎 [Researcher Agent](docs/RESEARCHER_AGENT.md)** — Complete guide to the research agent and inter-agent communication
- **📖 [Using the Libraries](docs/using-the-libraries.md)** — How to integrate into your own projects
- **🎯 [Samples Overview](docs/samples-overview.md)** — Complete guide to all four samples
- **💡 [Samples](samples/)** — Four ready-to-run examples:
  - **[ConsoleSimple](samples/ConsoleSimple)** — Minimal demo (weather app)
  - **[ConsoleCompleteChat](samples/ConsoleCompleteChat)** — Full interactive multi-turn chat
  - **[ConsoleFlowTraces](samples/ConsoleFlowTraces)** — Flow tracing and call graph visualization
  - **[AspireApp](samples/AspireApp)** — Production-grade Blazor dashboard with REST API

---

## Installation

```bash
dotnet add package ElBruno.AgentsOrchestration.Core
```

The package includes all three libraries (Abstractions, Orchestration, Core) and is powered by the **GitHub Copilot SDK** for agent intelligence.

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

## 🎬 Stay Updated & Connect

Love this library? Want to see how to use it in action? Check out my content across the web:

### 🚀 Follow the Journey

- **💻 [GitHub](https://github.com/elbruno/)** — Explore all my projects and contributions
- **📝 [Blog](https://elbruno.com)** — Deep dives into AI, orchestration, and .NET development
- **🎙️ [Podcast](https://notienenombre.com)** — Tech talks and interviews (Spanish 🇪🇸)
- **▶️ [YouTube](https://www.youtube.com/elbruno)** — Video tutorials and live coding sessions
- **💼 [LinkedIn](https://www.linkedin.com/in/elbruno/)** — Professional insights and updates
- **𝕏 [Twitter/X](https://www.x.com/in/elbruno/)** — Quick tips, updates, and community engagement

**Star ⭐ this repo if you find it helpful, and share it with your team!**

---

## 👋 About the Author

Hi! I'm **ElBruno**, a software architect and AI enthusiast passionate about building production-grade orchestration systems. I create tools that help developers leverage AI agents to automate complex workflows.

If you have questions, feedback, or want to chat about this project—hit me up on any of the platforms above! 🤝

---

## License

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**Copyright © ElBruno** — All rights reserved.
