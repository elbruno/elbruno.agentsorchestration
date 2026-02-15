using System.Text;
using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.Core.Orchestration;
using ElBruno.AgentsOrchestration.Orchestration;
using ElBruno.AgentsOrchestration.Views;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
Console.WriteLine("║  ElBruno.AgentsOrchestration - Flow & Trace Demo          ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

// Step 1: Create the orchestration service
var currentDir = Environment.CurrentDirectory;
var rootWorkspacePath = Directory.Exists(Path.Combine(currentDir, "samples"))
    ? Path.Combine(currentDir, "samples", "workspaces")
    : Path.Combine(currentDir, "..", "workspaces");
var service = OrchestrationServiceFactory.Create(
    rootWorkspacePath,
    autoApprovePlans: true,
    onPlanGenerated: plan =>
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n📋 PLAN GENERATED:");
        Console.ResetColor();
        Console.WriteLine(new string('─', 80));
        Console.WriteLine(plan);
        Console.WriteLine(new string('─', 80));
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✅ Auto-approving plan...\n");
        Console.ResetColor();
    });

// Step 2: Define a prompt that triggers research, coding, and potentially fixing logic
var prompt = "Create a .NET console app that displays current weather for three cities: Toronto, Tokyo, and Madrid. Search online for free services that can provide this information and use these services in the app to get the values for the temperature in these cities. Show the city name, temperature, and a weather emoji (☀️ for warm, 🌤️ for mild, ❄️ for cold).";

Console.WriteLine($"📋 Request: {prompt}\n");
Console.WriteLine("Running orchestration...");
Console.WriteLine(new string('─', 80));

// Step 3: Listen to events with verbose logging
var callGraph = new AgentCallGraph();
var eventTask = Task.Run(async () =>
{
    await foreach (var evt in service.Events.Reader.ReadAllAsync())
    {
        LogEvent(evt);
        TrackAgentCall(evt, callGraph);
    }
});

// Step 4: Run the orchestration
try
{
    var result = await service.RunAsync(
        new OrchestrationRequest(prompt),
        CancellationToken.None
    );

    service.Events.Writer.TryComplete();
    await eventTask;

    Console.WriteLine(new string('─', 80));
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n✅ ORCHESTRATION COMPLETE");
    Console.ResetColor();

    Console.WriteLine("\n📊 Agent Interaction Flow:");
    Console.WriteLine(callGraph.ToAsciiFlow());
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n❌ FATAL ERROR: {ex.Message}\n");
    Console.ResetColor();
}

// --------------------------------------------------------------------------------------
// Helper Methods
// --------------------------------------------------------------------------------------

void TrackAgentCall(OrchestrationEvent evt, AgentCallGraph graph)
{
    var now = DateTimeOffset.UtcNow;
    switch (evt)
    {
        case OrchestrationStartedEvent:
            // Implicit start
            break;
        case AgentActivatedEvent active:
            // Simplification: Assume Orchestrator calls everyone
            // In a real agentic system, agents might call each other directly
            graph.RecordCall(AgentRole.Orchestrator, active.Role, active.TaskDescription, now);
            break;
    }
}

void LogEvent(OrchestrationEvent evt)
{
    switch (evt)
    {
        case PhaseStartedEvent phase:
            Print("📌 PHASE START", $"Phase {phase.PhaseIndex}: {phase.PhaseName.ToUpper()}", ConsoleColor.White);
            break;

        case AgentActivatedEvent agent:
            Print("🤖 AGENT ACTIVE", $"{agent.Role,-18} | Task: {agent.TaskDescription}", ConsoleColor.Magenta);
            break;

        case AgentInstructionUpdateEvent update:
            Print("📝 PLAN UPDATE", $"Planner updated execution plan ({update.CurrentInstruction.Length} chars)", ConsoleColor.Yellow, indent: true);
            break;

        case AgentCompletedEvent completed:
            // Don't print full content, just success message
            // Print($"✅ AGENT DONE", $"{completed.Role} completed work.", ConsoleColor.DarkMagenta);
            break;

        case FileCreatedEvent file:
            Print("📄 FILE CREATED", file.FilePath, ConsoleColor.Yellow);
            break;

        case BuildValidationEvent build:
            if (build.Success)
            {
                Print("🔨 BUILD SUCCESS", "Project compiled successfully.", ConsoleColor.Green);
            }
            else
            {
                Print("💥 BUILD FAILED", "Compilation errors detected. Triggering self-healing...", ConsoleColor.Red);
                // Optional: Print build output if needed, but it's usually verbose
            }
            break;

        case FixAttemptStartedEvent fix:
            Print("🔧 SELF-HEALING", $"Attempt #{fix.Attempt} started...", ConsoleColor.Red);
            break;

        case FixAttemptCompletedEvent fixDone:
            var icon = fixDone.Success ? "✅" : "❌";
            var color = fixDone.Success ? ConsoleColor.Green : ConsoleColor.Red;
            Print($"🔧 FIX RESULT", $"{icon} Attempt #{fixDone.Attempt} {(fixDone.Success ? "SUCCEEDED" : "FAILED")}", color);
            break;

        case ResearchRequestedEvent research:
            Print("🔍 RESEARCHING", $"{research.Query} (Scope: {research.Scope})", ConsoleColor.Blue);
            break;

        case ResearchCompletedEvent researchDone:
            Print("📚 RESEARCH DONE", $"Found {researchDone.SourcesFound} sources in {researchDone.Duration.TotalMilliseconds:F0}ms", ConsoleColor.Blue);
            break;

        case BuildReviewStartedEvent:
            Print("🧐 REVIEW START", "Analyzing code quality...", ConsoleColor.DarkCyan);
            break;

        case BuildReviewCompletedEvent:
            Print("📝 REVIEW DONE", "Analysis complete.", ConsoleColor.DarkCyan);
            break;

        case OrchestrationCompletedEvent:
            Print("🏁 FINISHED", "All tasks completed.", ConsoleColor.Green);
            break;

        case OrchestrationErrorEvent err:
            Print("⚠️ ERROR", err.Error, ConsoleColor.Red);
            break;

        default:
            // Catch-all for other events
            Print("ℹ️ EVENT", evt.GetType().Name, ConsoleColor.DarkGray);
            break;
    }
}

void Print(string label, string message, ConsoleColor color, bool indent = false)
{
    Console.ForegroundColor = color;
    var prefix = indent ? "   " : "";
    Console.WriteLine($"{prefix}[{DateTime.Now:HH:mm:ss}] {label,-15} : {message}");
    Console.ResetColor();
}
