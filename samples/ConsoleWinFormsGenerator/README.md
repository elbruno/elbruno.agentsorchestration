# ConsoleWinFormsGenerator Sample

This sample demonstrates how to use the **AgentsOrchestration** library to automatically generate a Windows Forms application, with integrated build validation to ensure the generated code compiles successfully.

## Features

- **Dynamic Agent Loading**: Optionally loads the WinForms Expert agent from Awesome Copilot Repository
- **Full Application Generation**: Generates a complete, buildable WinForms project
- **Build Validation**: Automatically validates that generated code compiles
- **Error Reporting**: Shows detailed build errors if validation fails
- **Agent Integration**: Combines 11 core agents including Planner, Coder, Designer, Fixer, and BuildReviewer
- **Auto-Repair**: The Fixer agent automatically attempts to fix build errors during orchestration

## Running the Sample

```bash
cd samples/ConsoleWinFormsGenerator
dotnet run
```

## What It Does

1. **Initializes Core Agents** (11 default agents)
2. **Optionally Loads WinForms Expert** from Awesome Copilot Repository
3. **Generates a Simple Counter Application** with:
   - Main WinForms form titled "Counter App"
   - Label to display the counter value
   - Increment and Reset buttons
   - Clean, straightforward code structure
4. **Validates the Build** by attempting to compile the generated code
5. **Reports Results** with detailed error messages if build fails
6. **Provides Orchestration Summary** showing what was generated

## Expected Output

```
🪟 WinForms Application Generator with Dynamic Agent
====================================================

✅ Loaded 11 core agents

📦 Loading WinForms Expert agent...
✅ Loaded: WinForms Expert (👨‍💼)

🔨 Starting orchestration to generate WinForms app...

[Orchestration events...]

✨ Orchestration completed!

📁 Workspace: C:\Users\...\orchestration-workspaces\20260217005325-create-a-simple-windows-forms/
📋 Generated 5 tasks

🔍 Validating generated code build...
✅ Code builds successfully!

🎉 Your WinForms application is ready!
📦 To run it:
   cd C:\Users\...\orchestration-workspaces\20260217005325-create-a-simple-windows-forms/
   dotnet run
```

## Build Validation

The sample now includes automatic build validation that:

1. **Locates** the generated `.csproj` file
2. **Attempts** to compile the generated code using `dotnet build`
3. **Parses** compiler errors if the build fails
4. **Reports** detailed error messages to help with debugging

If the build fails, you can:

- Review the workspace files manually
- Check the Fixer agent's output in the orchestration summary
- Edit the files manually and rebuild

## How It Works

### 1. Agent Coordination

The **Orchestrator** delegates tasks to specialized agents:

- **Planner** — Creates implementation plan for the Counter application
- **Coder** — Generates C# code (Program.cs, MainForm.cs)
- **Designer** — Creates form layouts and control configurations
- **Fixer** — Automatically attempts to fix build errors
- **BuildReviewer** — Analyzes code quality and provides feedback

### 2. 6-Step Pipeline

| Step | Agent | Task |
|------|-------|------|
| 1. Plan | Planner | Create implementation plan |
| 2. Parse | Orchestrator | Parse plan, create task phases |
| 3. Execute | Coder / Designer | Generate code and forms |
| 4. Verify | Fixer | Build validation, auto-repair |
| 5. Review | BuildReviewer | Analyze quality and patterns |
| 5. Review | BuildReviewer | Analyze quality and patterns |
| 6. Report | Orchestrator | Final summary |

## Generated Application Structure

When successful, the sample generates a buildable WinForms project with:

```
workspace/
├── Program.cs           # Application entry point with Main()
├── MainForm.cs          # Main form definition
├── MainForm.Designer.cs # Designer-generated UI code
├── project.csproj       # WinForms project configuration
└── bin/                 # Build output
```

## Learn More

- [AddAndListCustomAgents](../AddAndListCustomAgents) — Learn dynamic agent loading basics
- [Awesome Copilot Repository](https://github.com/github/awesome-copilot)
- [Main Documentation](../../README.md)
- [Dynamic Agents Guide](../../docs/DYNAMIC_AGENTS.md)
