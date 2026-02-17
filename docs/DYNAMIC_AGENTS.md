# Dynamic Agent Loading

The AgentRepository library enables you to extend your orchestration system with specialized agents from external sources, particularly the [Awesome Copilot Repository](https://github.com/github/awesome-copilot).

## Overview

Dynamic agent loading allows you to:

- **Load agents at runtime** from GitHub, URLs, or local files
- **Extend the 11 built-in agents** with domain-specific experts
- **Cache downloaded agents** for faster subsequent loads
- **Manage agent lifecycle** (load, unload, query)
- **Integrate seamlessly** with the existing orchestration system

## Quick Start

### Basic Usage

```csharp
using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.AgentRepository.Services;

// Initialize
var staticStore = new AgentConfigurationStore();
var loader = new AwesomeCopilotAgentLoader();
var manager = new DynamicAgentManager(staticStore, loader);

// Load an agent from Awesome Copilot Repository
var winFormsAgent = await manager.LoadAndRegisterAgentAsync("WinFormsExpert");

Console.WriteLine($"Loaded: {winFormsAgent.Name}");
Console.WriteLine($"Instructions: {winFormsAgent.Instructions[..100]}...");
```

### With Dependency Injection

```csharp
// In Program.cs or Startup.cs
services.AddOrchestration()
    .AddDynamicAgents(options =>
    {
        options.CacheDirectory = "./agent-cache";
        options.CacheExpirationHours = 24;
    });

// In your service or controller
public class MyService
{
    private readonly DynamicAgentManager _agentManager;
    
    public MyService(DynamicAgentManager agentManager)
    {
        _agentManager = agentManager;
    }
    
    public async Task InitializeSpecializedAgents()
    {
        await _agentManager.LoadAndRegisterAgentAsync("WinFormsExpert");
        await _agentManager.LoadAndRegisterAgentAsync("se-security-reviewer");
    }
}
```

## Loading Agents

### From Awesome Copilot Repository

```csharp
// Load by agent ID (filename without extension)
var agent = await manager.LoadAndRegisterAgentAsync("WinFormsExpert");
```

Available agents in Awesome Copilot Repository:

- `WinFormsExpert` - Windows Forms development expert
- `se-security-reviewer` - Security-focused code reviewer
- See [full list](https://github.com/github/awesome-copilot/tree/main/agents)

### From URL

```csharp
var customAgent = await manager.LoadAndRegisterAgentFromUrlAsync(
    "https://raw.githubusercontent.com/user/repo/main/my-agent.agent.md",
    agentId: "CustomAgent"
);
```

### From Local File

```csharp
var localAgent = await manager.LoadAndRegisterAgentFromFileAsync(
    "/path/to/my-agent.agent.md"
);
```

## Agent Definition Format

Dynamic agents use the `.agent.md` format with YAML front matter:

```markdown
---
name: My Custom Agent
description: A specialized agent for domain-specific tasks
model: gpt-5
version: 1.0
tools: ['codebase', 'search', 'edit/editFiles']
---

# My Custom Agent

## Your Mission

Your role is to...

## Instructions

1. Analyze the task
2. Provide specific guidance
3. ...
```

### Front Matter Fields

- **name** (required): Display name of the agent
- **description** (required): Brief description of the agent's purpose
- **model** (optional): LLM model to use (defaults to `gpt-5.3-codex`)
- **version** (optional): Version string
- **tools** (optional): Array of tool names the agent can use

## Managing Agents

### Query Agents

```csharp
// Get all agents (static + dynamic)
var allAgents = manager.GetAllAgents();

// Get agent by name
var config = manager.GetByName("WinForms Expert");

// Check if agent exists
bool hasAgent = manager.HasAgent("SE: Security");

// Get only dynamic agent names
var dynamicNames = manager.GetDynamicAgentNames();
```

### Remove Agents

```csharp
// Remove a dynamic agent
bool removed = manager.RemoveDynamicAgent("WinForms Expert");
```

Note: You cannot remove static agents (the 11 built-in agents).

## Configuration Options

```csharp
var options = new AgentRepositoryOptions
{
    // Base URL for the Awesome Copilot Repository
    RepositoryUrl = "https://raw.githubusercontent.com/github/awesome-copilot/main/agents/",
    
    // Local cache directory for downloaded agents
    CacheDirectory = "./agent-cache",
    
    // How long to cache agents before re-downloading (in hours)
    CacheExpirationHours = 24,
    
    // HTTP timeout for downloads (in seconds)
    TimeoutSeconds = 30
};

var loader = new AwesomeCopilotAgentLoader(options);
```

## Use Case Scenarios

### Scenario 1: WinForms Development

When building a Windows Forms application, load the WinForms Expert to:

```csharp
// Load the expert
await manager.LoadAndRegisterAgentAsync("WinFormsExpert");

// The WinForms Expert can now:
// - Validate Designer-compatible code
// - Ensure proper MVVM binding patterns
// - Review InitializeComponent methods
// - Check for DarkMode support
// - Validate HighDPI settings
```

The agent's instructions cover:

- .NET 10+ project setup
- Designer code context (what's allowed/prohibited)
- MVVM binding requirements
- NuGet package best practices
- VB.NET specifics

### Scenario 2: Security Review

Add security-focused review to your pipeline:

```csharp
// Load the security reviewer
await manager.LoadAndRegisterAgentAsync("se-security-reviewer");

// The Security Reviewer can:
// - Check for OWASP Top 10 vulnerabilities
// - Review LLM security (prompt injection, etc.)
// - Validate Zero Trust implementation
// - Check cryptographic implementations
// - Review authentication/authorization
```

The agent provides:

- Targeted review plans based on code type
- Specific vulnerability checks
- Code examples for fixes
- Priority-based recommendations

### Scenario 3: Project-Specific Experts

Create and load your own agents:

```csharp
// my-project-expert.agent.md
var projectExpert = await manager.LoadAndRegisterAgentFromFileAsync(
    "./agents/my-project-expert.agent.md"
);

// Now use it in orchestration
var factory = new DynamicAgentFactory(staticFactory, agentClient, manager);
var session = factory.CreateSessionByName("My Project Expert");
```

## Integration with Orchestration

### Using Dynamic Agents in Orchestration

```csharp
// Create an extended factory that supports both static and dynamic agents
var dynamicFactory = new DynamicAgentFactory(
    staticFactory: new AgentFactory(staticStore, agentClient),
    agentClient: agentClient,
    dynamicManager: manager
);

// Create sessions for dynamic agents
var winFormsSession = dynamicFactory.CreateSessionByName("WinForms Expert");
var securitySession = dynamicFactory.CreateSessionByName("SE: Security");

// Use in orchestration
if (winFormsSession != null)
{
    var result = await winFormsSession.RunAsync(
        "Review this WinForms code for Designer compatibility",
        workspacePath,
        cancellationToken
    );
}
```

### Auto-Discovery Pattern

```csharp
// Load agents based on project characteristics
public async Task LoadRelevantAgents(string projectType)
{
    if (projectType.Contains("WinForms"))
    {
        await manager.LoadAndRegisterAgentAsync("WinFormsExpert");
    }
    
    if (projectType.Contains("Security"))
    {
        await manager.LoadAndRegisterAgentAsync("se-security-reviewer");
    }
    
    // List all available agents
    var agents = manager.GetAllAgents();
    Console.WriteLine($"Loaded {agents.Count} agents for {projectType} project");
}
```

## Creating Custom Agents

### 1. Create the Agent Definition

```markdown
---
name: Database Expert
description: Specialized in database design, optimization, and migrations
model: gpt-5.3-codex
version: 1.0
tools: ['codebase', 'search']
---

# Database Expert

## Your Mission

Review and optimize database schemas, queries, and migrations.

## Key Responsibilities

1. **Schema Design**: Ensure proper normalization and indexing
2. **Query Optimization**: Identify and fix N+1 queries, missing indexes
3. **Migrations**: Validate migration safety and rollback strategies
4. **Performance**: Check connection pooling, query performance

## Review Checklist

- [ ] Schema follows 3NF (unless denormalized intentionally)
- [ ] Indexes on foreign keys and frequently queried columns
- [ ] No N+1 query patterns
- [ ] Migrations are reversible
- [ ] Connection pooling configured
```

### 2. Load and Test

```csharp
var dbExpert = await manager.LoadAndRegisterAgentFromFileAsync(
    "./agents/database-expert.agent.md"
);

Console.WriteLine($"Loaded: {dbExpert.Name}");
Console.WriteLine($"Description: {dbExpert.Description}");
```

### 3. Share with Community

Consider contributing your agent to the [Awesome Copilot Repository](https://github.com/github/awesome-copilot)!

## Best Practices

### 1. Cache Management

```csharp
// Use caching in production
var options = new AgentRepositoryOptions
{
    CacheDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "AgentOrchestration",
        "cache"
    ),
    CacheExpirationHours = 24 * 7 // 1 week
};
```

### 2. Error Handling

```csharp
try
{
    var agent = await manager.LoadAndRegisterAgentAsync("WinFormsExpert");
}
catch (HttpRequestException ex)
{
    // Network error or agent not found
    Console.WriteLine($"Failed to load agent: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    // Parsing error
    Console.WriteLine($"Invalid agent definition: {ex.Message}");
}
```

### 3. Pre-loading Agents

```csharp
// Pre-load commonly used agents at startup
public async Task PreloadAgents(DynamicAgentManager manager)
{
    var commonAgents = new[] { "WinFormsExpert", "se-security-reviewer" };
    
    await Task.WhenAll(
        commonAgents.Select(id => 
            manager.LoadAndRegisterAgentAsync(id))
    );
}
```

### 4. Agent Versioning

```csharp
// Check agent version before using
var agent = manager.GetByName("WinForms Expert");
if (agent?.Metadata?.TryGetValue("Version", out var version) == true)
{
    Console.WriteLine($"Using WinForms Expert v{version}");
}
```

## Troubleshooting

### Agent Not Found

```
❌ Failed to load agent 'XYZ' from URL. Ensure the agent exists in the repository.
```

**Solution**: Check the [Awesome Copilot agents directory](https://github.com/github/awesome-copilot/tree/main/agents) for available agents.

### Cache Issues

```csharp
// Clear cache manually
if (Directory.Exists(options.CacheDirectory))
{
    Directory.Delete(options.CacheDirectory, recursive: true);
}
```

### Network Timeouts

```csharp
// Increase timeout
var options = new AgentRepositoryOptions
{
    TimeoutSeconds = 60 // 1 minute
};
```

## API Reference

See the full API documentation:

- [AwesomeCopilotAgentLoader](../src/ElBruno.AgentsOrchestration.AgentRepository/Services/AwesomeCopilotAgentLoader.cs)
- [DynamicAgentManager](../src/ElBruno.AgentsOrchestration.AgentRepository/Services/DynamicAgentManager.cs)
- [AgentDefinitionParser](../src/ElBruno.AgentsOrchestration.AgentRepository/Services/AgentDefinitionParser.cs)

## Examples

- [AddAndListCustomAgents](../samples/AddAndListCustomAgents) - Complete working example
- [Awesome Copilot Repository](https://github.com/github/awesome-copilot) - Community agent definitions

## Next Steps

- Explore [available agents](https://github.com/github/awesome-copilot/tree/main/agents)
- Create your own custom agents
- Contribute agents to the community
- Integrate dynamic agents into your orchestration pipeline
