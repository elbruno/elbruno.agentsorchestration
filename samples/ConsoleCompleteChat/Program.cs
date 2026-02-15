using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.Core.Orchestration;
using ElBruno.AgentsOrchestration.Orchestration;
using ElBruno.AgentsOrchestration.Workspace;

// Ensure we always target the 'samples/workspaces' folder relative to the repository root
// This helps with visibility of generated files
var currentDir = Environment.CurrentDirectory;
var rootPath = Directory.Exists(Path.Combine(currentDir, "samples"))
    ? Path.Combine(currentDir, "samples", "workspaces")
    : Path.Combine(currentDir, "..", "workspaces");
// Fallback if we are not where we expect to be (e.g. running from bin folder differently)
if (!Directory.Exists(Path.Combine(rootPath, "..")))
    rootPath = Path.Combine(Path.GetTempPath(), "elbr-orchestration-demo");

try
{
    Console.Clear();
}
catch
{
    // In piped/non-interactive contexts, Clear() may fail; continue anyway
}

Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
Console.WriteLine("║  ElBruno.AgentsOrchestration - Interactive Console Demo     ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

// Initialize components
var store = new AgentConfigurationStore(InstructionLoader.LoadInstructions());
var client = new TemplateAgentClient();
var factory = new AgentFactory(store, client);
var workspaceManager = new WorkspaceManager(rootPath);
var conversationManager = new ConversationManager();

var running = true;
ConversationSession? currentSession = null;
string? currentWorkspacePath = null;
AppRunner? appRunner = null;

// Define all helper functions before main loop
async Task RunNewOrchestrationAsync()
{
    try
    {
        Console.Write("\nEnter your request (or press Enter for default): ");
        var prompt = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(prompt))
        {
            prompt = "Create a .NET console application that prints 'Hello, World!'";
        }

        Console.WriteLine($"\n📋 Request: {prompt}\n");

        // Create session and workspace
        currentWorkspacePath = workspaceManager.CreateWorkspace(prompt);
        currentSession = conversationManager.CreateSession(currentWorkspacePath);

        await RunOrchestrationAsync(prompt);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Error starting orchestration: {ex.Message}\n");
        currentSession = null;
        currentWorkspacePath = null;
    }
}

async Task RunNextTurnAsync()
{
    try
    {
        if (currentSession is null || currentWorkspacePath is null)
        {
            Console.WriteLine("❌ No active session. Please start a new orchestration first.\n");
            return;
        }

        Console.Write("\nWhat would you like to change or add? ");
        var newPrompt = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(newPrompt))
        {
            Console.WriteLine("❌ Prompt cannot be empty.\n");
            return;
        }

        // Build context from conversation history
        var contextPrompt = currentSession.BuildContextPrompt(newPrompt);

        Console.WriteLine($"\n📋 Request: {newPrompt}\n");
        await RunOrchestrationAsync(contextPrompt);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Error: {ex.Message}\n");
    }
}

async Task RunOrchestrationAsync(string prompt)
{
    var service = new OrchestrationService(factory, new WorkspaceManager(rootPath));

    // Auto-approve plans in console mode
    service.PlanApprovalCallback = (plan, markdown) =>
    {
        Console.WriteLine("\n📋 Plan Generated:");
        Console.WriteLine(new string('─', 60));
        Console.WriteLine(markdown);
        Console.WriteLine(new string('─', 60));
        Console.WriteLine("✅ Auto-approving plan...\n");
        return Task.FromResult(true);
    };

    Console.WriteLine("Running orchestration...");
    Console.WriteLine(new string('─', 60));

    var eventTask = Task.Run(async () =>
    {
        await foreach (var evt in service.Events.Reader.ReadAllAsync())
        {
            LogEvent(evt);
        }
    });

    try
    {
        var result = await service.RunAsync(new OrchestrationRequest(prompt), CancellationToken.None);
        service.Events.Writer.TryComplete();
        await eventTask;

        Console.WriteLine(new string('─', 60));
        Console.WriteLine($"\n✅ Orchestration Complete!");
        Console.WriteLine($"Summary: {result.Summary}");

        // Record turn in conversation
        if (currentSession is not null)
        {
            currentSession = conversationManager.RecordTurn(currentSession.SessionId, prompt, result);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Error: {ex.Message}");
    }

    Console.WriteLine();
}

async Task LaunchAppAsync()
{
    try
    {
        if (currentWorkspacePath is null)
        {
            Console.WriteLine("❌ No workspace loaded.\n");
            return;
        }

        Console.WriteLine("\n🚀 Launching application...\n");

        appRunner = new AppRunner();
        appRunner.OnLogReceived += line =>
        {
            Console.WriteLine($"[App] {line}");
        };

        var launched = await appRunner.LaunchAsync(currentWorkspacePath, CancellationToken.None);

        if (launched)
        {
            Console.WriteLine("\n✅ App running. Press Enter to stop it...");
            Console.ReadLine();
            await appRunner.StopAsync();
            Console.WriteLine("⏹️ App stopped.\n");
        }
        else
        {
            Console.WriteLine("❌ Failed to launch app. No .csproj files found in workspace.\n");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Error launching app: {ex.Message}\n");
    }
}

void ViewGeneratedFiles()
{
    try
    {
        if (currentWorkspacePath is null)
        {
            Console.WriteLine("❌ No workspace loaded.\n");
            return;
        }

        Console.WriteLine($"\n📁 Files in: {currentWorkspacePath}\n");

        var files = Directory.GetFiles(currentWorkspacePath, "*", SearchOption.AllDirectories)
            .Select(f => f.Replace(currentWorkspacePath, "").TrimStart(Path.DirectorySeparatorChar))
            .OrderBy(f => f)
            .ToList();

        if (files.Count == 0)
        {
            Console.WriteLine("No files generated yet.\n");
        }
        else
        {
            foreach (var file in files)
            {
                Console.WriteLine($"  📄 {file}");
            }
            Console.WriteLine();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error listing files: {ex.Message}\n");
    }
}

void ShowRunCommand()
{
    try
    {
        if (currentWorkspacePath is null)
        {
            Console.WriteLine("❌ No workspace loaded.\n");
            return;
        }

        var projectFiles = Directory.GetFiles(currentWorkspacePath, "*.csproj", SearchOption.TopDirectoryOnly);

        if (projectFiles.Length == 0)
        {
            Console.WriteLine("❌ No .csproj file found in workspace.\n");
            return;
        }

        var projectFile = Path.GetFileName(projectFiles[0]);
        var runCommand = $"dotnet run --project \"{currentWorkspacePath}\\{projectFile}\"";

        Console.WriteLine("\n📋 Run Command:");
        Console.WriteLine($"\n  {runCommand}\n");
        Console.WriteLine("💡 Copy the command above to run your application.\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error generating run command: {ex.Message}\n");
    }
}

void ListSessions()
{
    try
    {
        var sessions = conversationManager.ListSessions();
        Console.WriteLine($"\n📊 Active Sessions: {sessions.Count}\n");

        if (sessions.Count == 0)
        {
            Console.WriteLine("No active sessions.\n");
            return;
        }

        for (int i = 0; i < sessions.Count; i++)
        {
            var s = sessions[i];
            Console.WriteLine($"  [{i + 1}] {Path.GetFileName(s.WorkspacePath)}");
            Console.WriteLine($"      Created: {s.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"      Turns: {s.History.Count}");
        }
        Console.WriteLine();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error listing sessions: {ex.Message}\n");
    }
}

void DisplayMainMenu()
{
    Console.WriteLine("Select an option:");
    Console.WriteLine("  [1] Start new orchestration");
    Console.WriteLine("  [2] List active sessions");
    Console.WriteLine("  [3] Exit");
    Console.Write("\nChoice: ");
}

void DisplayConversationMenu()
{
    Console.WriteLine($"\n📊 Current Session: {Path.GetFileName(currentWorkspacePath)}");
    Console.WriteLine($"   Turns: {currentSession?.History.Count ?? 0}\n");
    Console.WriteLine("What would you like to do?");
    Console.WriteLine("  [1] Add feature / make changes");
    Console.WriteLine("  [2] Run the app");
    Console.WriteLine("  [3] View generated files");
    Console.WriteLine("  [4] Show run command (copy to clipboard)");
    Console.WriteLine("  [5] Start new orchestration");
    Console.WriteLine("  [6] Exit");
    Console.Write("\nChoice: ");
}

async Task CleanupAsync()
{
    if (appRunner is not null)
    {
        await appRunner.StopAsync();
        appRunner.Dispose();
    }
}

void LogEvent(OrchestrationEvent evt)
{
    var message = evt switch
    {
        OrchestrationStartedEvent started => $"🚀 Started: {started.Prompt[..Math.Min(60, started.Prompt.Length)]}...",
        PhaseStartedEvent phase => $"📌 Phase {phase.PhaseIndex}: {phase.PhaseName}",
        AgentActivatedEvent agent => $"🤖 {agent.Role}: {agent.TaskDescription[..Math.Min(50, agent.TaskDescription.Length)]}...",
        AgentCompletedEvent completed => $"✅ {completed.Role} completed",
        FileCreatedEvent file => $"📄 File: {file.FilePath}",
        BuildValidationEvent build => build.Success ? "🔨 Build succeeded" : "❌ Build failed",
        FixAttemptStartedEvent fix => $"🔧 Fix attempt {fix.Attempt}",
        FixAttemptCompletedEvent fix => fix.Success ? $"✅ Fix {fix.Attempt} succeeded" : $"❌ Fix {fix.Attempt} failed",
        OrchestrationCompletedEvent => "🏁 Orchestration completed",
        OrchestrationErrorEvent error => $"⚠️ Error: {error.Error}",
        _ => $"ℹ️ {evt.GetType().Name}"
    };
    Console.WriteLine(message);
}

// MAIN LOOP
try
{
    while (running)
    {
        try
        {
            // Display menu based on current state
            if (currentSession is null)
            {
                DisplayMainMenu();
                var choice = Console.ReadLine()?.Trim().ToLower() ?? "";

                switch (choice)
                {
                    case "1":
                        await RunNewOrchestrationAsync();
                        break;
                    case "2":
                        ListSessions();
                        break;
                    case "3":
                        running = false;
                        Console.WriteLine("\n👋 Goodbye!\n");
                        break;
                    default:
                        Console.WriteLine("❌ Invalid option. Try again.\n");
                        break;
                }
            }
            else
            {
                DisplayConversationMenu();
                var choice = Console.ReadLine()?.Trim().ToLower() ?? "";

                switch (choice)
                {
                    case "1":
                        await RunNextTurnAsync();
                        break;
                    case "2":
                        await LaunchAppAsync();
                        break;
                    case "3":
                        ViewGeneratedFiles();
                        break;
                    case "4":
                        ShowRunCommand();
                        break;
                    case "5":
                        currentSession = null;
                        currentWorkspacePath = null;
                        Console.WriteLine("\n✅ Session cleared.\n");
                        break;
                    case "6":
                        running = false;
                        await CleanupAsync();
                        Console.WriteLine("\n👋 Goodbye!\n");
                        break;
                    default:
                        Console.WriteLine("❌ Invalid option. Try again.\n");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n⚠️ Unexpected error: {ex.Message}\n");
            // Continue the loop instead of crashing
        }
    }

    await CleanupAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ Fatal error: {ex.Message}\n");
    Environment.Exit(1);
}
