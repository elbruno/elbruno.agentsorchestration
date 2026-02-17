# AddAndListCustomAgents Sample

This sample demonstrates how to dynamically load and register custom agent definitions from the [Awesome Copilot Repository](https://github.com/github/awesome-copilot) and integrate them into the orchestration system.

## Features

- **Dynamic Agent Loading**: Load agent definitions at runtime from GitHub
- **Agent Management**: Register, list, and query both static and dynamic agents
- **Caching**: Downloaded agents are cached locally for faster subsequent loads
- **Flexible Sources**: Load agents from URLs, files, or the Awesome Copilot Repository

## Running the Sample

```bash
cd samples/AddAndListCustomAgents
dotnet run
```

## What It Does

1. **Initializes Static Agents**: Loads the 11 built-in agents (Orchestrator, Planner, Coder, etc.)
2. **Loads WinForms Expert**: Downloads and registers the WinForms Expert agent from Awesome Copilot
3. **Loads Security Reviewer**: Downloads and registers the Security Reviewer agent
4. **Lists All Agents**: Shows both static (⚙️) and dynamic (🔌) agents
5. **Demonstrates Scenarios**: Shows how dynamic agents can be used for specific tasks

## Expected Output

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

## Use Cases

### Scenario 1: WinForms Project

When building a WinForms application, the WinForms Expert agent can be loaded to:

- Validate WinForms Designer-compatible code
- Ensure proper MVVM binding patterns
- Review InitializeComponent methods

### Scenario 2: Security Review

The Security Reviewer can be added to the pipeline to:

- Review code for OWASP Top 10 vulnerabilities
- Check LLM security (prompt injection, etc.)
- Enforce Zero Trust principles

## API Usage

```csharp
// Initialize
var staticStore = new AgentConfigurationStore();
var loader = new AwesomeCopilotAgentLoader();
var manager = new DynamicAgentManager(staticStore, loader);

// Load from repository
var agent = await manager.LoadAndRegisterAgentAsync("WinFormsExpert");

// Load from URL
var customAgent = await manager.LoadAndRegisterAgentFromUrlAsync(
    "https://raw.githubusercontent.com/user/repo/main/my-agent.agent.md"
);

// Load from local file
var localAgent = await manager.LoadAndRegisterAgentFromFileAsync(
    "/path/to/agent.agent.md"
);

// Query agents
var config = manager.GetByName("WinForms Expert");
var hasAgent = manager.HasAgent("SE: Security");
var allAgents = manager.GetAllAgents();
```

## Configuration Options

```csharp
var options = new AgentRepositoryOptions
{
    RepositoryUrl = "https://raw.githubusercontent.com/github/awesome-copilot/main/agents/",
    CacheDirectory = "/path/to/cache",
    CacheExpirationHours = 24,
    TimeoutSeconds = 30
};

var loader = new AwesomeCopilotAgentLoader(options);
```

## Integration with Dependency Injection

```csharp
services.AddOrchestration()
    .AddDynamicAgents(options =>
    {
        options.CacheDirectory = "./agent-cache";
        options.CacheExpirationHours = 48;
    });

// Then resolve and use
var manager = serviceProvider.GetRequiredService<DynamicAgentManager>();
await manager.LoadAndRegisterAgentAsync("WinFormsExpert");
```

## Learn More

- [ConsoleWinFormsGenerator](../ConsoleWinFormsGenerator) — See dynamic agents in action generating applications
- [Awesome Copilot Repository](https://github.com/github/awesome-copilot)
- [Available Agents](https://github.com/github/awesome-copilot/blob/main/docs/README.agents.md)
- [Main Documentation](../../README.md)
- [Dynamic Agents Guide](../../docs/DYNAMIC_AGENTS.md)
