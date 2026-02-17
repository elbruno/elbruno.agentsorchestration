# ConsoleWinFormsGenerator Sample

This sample demonstrates how to use the **AgentsOrchestration** library with a dynamically loaded WinForms Expert agent to automatically generate a complete Windows Forms application.

## Features

- **Dynamic Agent Loading**: Loads the WinForms Expert agent from the Awesome Copilot Repository
- **Full Application Generation**: Generates a complete WinForms application (UI, models, data persistence)
- **Agent Integration**: Combines 11 core agents with domain-specific expertise from the WinForms Expert
- **Error Handling**: Gracefully handles failures if dynamic agent loading is unavailable
- **Real-Time Progress**: Shows orchestration events and progress

## Running the Sample

```bash
cd samples/ConsoleWinFormsGenerator
dotnet run
```

## What It Does

1. **Initializes Core Agents** (11 default agents including Planner, Coder, Designer, Fixer, BuildReviewer)
2. **Dynamically Loads WinForms Expert** from the Awesome Copilot Repository
3. **Generates a Complete Todo Manager Application** with:
   - Main WinForms window with professional styling
   - ListView for task management
   - Add/Edit/Delete functionality
   - Task completion tracking
   - JSON-based persistence
   - MVVM pattern with proper separation of concerns
   - Input validation and error handling
4. **Validates the Build** and auto-fixes any issues
5. **Provides Build Review** feedback on code quality and best practices
6. **Outputs Ready-to-Run Application** in a timestamped workspace

## Expected Output

```
🪟 WinForms Application Generator with Dynamic Agent
====================================================

✅ Loaded 11 core agents

📦 Loading WinForms Expert agent...
✅ Loaded: WinForms Expert (👨‍💼)

🔨 Starting orchestration to generate WinForms application...

📝 Prompt: Create a professional WinForms application with the following features:...

[Events streaming real-time progress...]

✨ Orchestration completed!

📁 Workspace: D:\elbruno\elbruno.agentsorchestration\samples\workspaces\20260216143022-create-a-professional-winforms/
📊 Status: CompletedSuccessfully

📄 Summary:
✅ Successfully generated a complete WinForms Todo Manager application

🎉 Your WinForms application is ready!
📦 To run it:
   cd D:\elbruno\elbruno.agentsorchestration\samples\workspaces\20260216143022-create-a-professional-winforms/
   dotnet run
```

## Generated Application Structure

The orchestration generates a complete, buildable WinForms project with:

```
workspace/
├── Program.cs           # Application entry point
├── MainForm.cs          # Main UI form
├── MainForm.Designer.cs # Designer-generated UI code
├── TaskModel.cs         # Data model
├── TaskRepository.cs    # Data persistence
├── project.csproj       # WinForms project configuration
└── bin/                 # Build output
```

## How It Works

### 1. Agent Coordination

The **Orchestrator** delegates tasks to specialized agents:

- **Planner** — Creates a detailed implementation plan
- **Designer** — Creates form layouts and UI structure
- **Coder** — Implements business logic and event handlers
- **WinForms Expert** (dynamic) — Validates WinForms patterns and best practices
- **Fixer** — Corrects any build errors automatically
- **BuildReviewer** — Analyzes code quality and provides recommendations

### 2. 6-Step Pipeline

| Step | Agent | Task |
|------|-------|------|
| 1. Plan | 🗺️ Planner | Understand requirements, create implementation plan |
| 2. Parse | 🧭 Orchestrator | Parse plan, create task phases |
| 3. Execute | 💻 Coder / 🎨 Designer | Generate code and UI |
| 4. Verify | 🔧 Fixer | Build validation, auto-repair |
| 5. Review | 📊 BuildReviewer | Analyze quality, security, patterns |
| 6. Report | 🧭 Orchestrator | Summarize results |

### 3. Dynamic Agent Role

The **WinForms Expert** provides specialized guidance on:

- Windows Forms Designer compatibility
- MVVM patterns for WinForms
- Windows Forms best practices
- Control initialization and binding
- Event handler patterns
- Resource management

## API Usage

```csharp
// Initialize services
var staticStore = new AgentConfigurationStore();
var loader = new AwesomeCopilotAgentLoader();
var manager = new DynamicAgentManager(staticStore, loader);

// Load specialized agent
var winFormsAgent = await manager.LoadAndRegisterAgentAsync("WinFormsExpert");

// Create orchestration service
var service = OrchestrationServiceFactory.Create();

// Run orchestration with custom prompt
var result = await service.RunAsync(
    new OrchestrationRequest("Your WinForms requirements..."),
    CancellationToken.None
);

// Access generated files
Console.WriteLine($"Generated in: {result.WorkspacePath}");
Console.WriteLine($"Summary: {result.Summary}");
```

## Customization

### Different Application Types

Modify the prompt in `Program.cs` to generate different WinForms applications:

```csharp
// Generate a data entry form
var prompt = @"Create a WinForms application for customer data entry...";

// Generate a reporting dashboard
var prompt = @"Create a WinForms dashboard with charts and real-time data...";

// Generate a document editor
var prompt = @"Create a rich text editor WinForms application...";
```

### Load Different Agents

Extend the sample to load additional specialized agents:

```csharp
// Load database expert
var dbAgent = await manager.LoadAndRegisterAgentAsync("DatabaseExpert");

// Load UI/UX expert  
var uiAgent = await manager.LoadAndRegisterAgentAsync("UIUXExpert");

// Load performance expert
var perfAgent = await manager.LoadAndRegisterAgentAsync("PerformanceExpert");
```

### Conditional Agent Loading

Handle cases where agents may not be available:

```csharp
try
{
    var specializedAgent = await manager.LoadAndRegisterAgentAsync("SpecializedAgent");
    Console.WriteLine($"✅ Using specialized agent: {specializedAgent.Name}");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️  Using standard agents: {ex.Message}");
}
```

## Troubleshooting

### Build Errors

If the generated application doesn't compile:

1. Check the workspace logs
2. Verify WinForms framework is installed: `dotnet workload install windowsdesktop`
3. The Fixer agent will automatically attempt repairs

### Missing Dynamic Agent

If the WinForms Expert fails to load:

1. Check internet connectivity for Awesome Copilot Repository
2. Verify agent is available: <https://github.com/github/awesome-copilot>
3. The sample continues with standard 11 agents

### Network Issues

Configure timeout and caching:

```csharp
var options = new AgentRepositoryOptions
{
    TimeoutSeconds = 60,
    CacheExpirationHours = 24
};

var loader = new AwesomeCopilotAgentLoader(options);
```

## Learn More

- [AddAndListCustomAgents](../AddAndListCustomAgents) — Learn dynamic agent loading basics
- [Awesome Copilot Repository](https://github.com/github/awesome-copilot)
- [Main Documentation](../../README.md)
- [Dynamic Agents Guide](../../docs/DYNAMIC_AGENTS.md)
