using ElBruno.AgentsOrchestration.Core.Orchestration;
using ElBruno.AgentsOrchestration.Orchestration;

Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
Console.WriteLine("║  ElBruno.AgentsOrchestration - Simple Console Demo        ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

// Step 1: Create the orchestration service with a custom workspace path
// Ensure we always target the 'samples/workspaces' folder relative to the repository root
var currentDir = Environment.CurrentDirectory;
var rootWorkspacePath = Directory.Exists(Path.Combine(currentDir, "samples"))
    ? Path.Combine(currentDir, "samples", "workspaces")
    : Path.Combine(currentDir, "..", "workspaces");

var service = OrchestrationServiceFactory.Create(rootWorkspacePath);

// Step 2: Define the prompt
var prompt = "Create a .NET console app that displays current weather for three cities: London, Tokyo, and New York. Use random temperatures between 10 and 30 degrees Celsius. Show the city name, temperature, and a weather emoji (☀️ for warm, 🌤️ for mild, ❄️ for cold).";

// var prompt = "Create a .NET console app that displays current weather for three cities: Toronto, Tokyo, and Madrid. Search online for free services that can provide this information and use these services in the app to get the values for the temperature in these cities. Show the city name, temperature, and a weather emoji (☀️ for warm, 🌤️ for mild, ❄️ for cold).";

Console.WriteLine($"📋 Request: {prompt}\n");
Console.WriteLine("Running orchestration...");
Console.WriteLine(new string('─', 60));

// Step 3: Listen to events (optional but helpful for visibility)
var eventTask = Task.Run(async () =>
{
    await foreach (var evt in service.Events.Reader.ReadAllAsync())
    {
        var message = evt switch
        {
            OrchestrationStartedEvent => "🚀 Orchestration started",
            PhaseStartedEvent phase => $"\t📌 Phase {phase.PhaseIndex}: {phase.PhaseName}",
            AgentActivatedEvent agent => $"🤖 {agent.Role}",
            FileCreatedEvent file => $"\t📄 Created: {Path.GetFileName(file.FilePath)}",
            BuildValidationEvent build => build.Success ? "\t✅ Build succeeded" : "\t❌ Build failed",
            OrchestrationCompletedEvent => "🏁 Completed",
            _ => null
        };

        if (message is not null)
        {
            // add the timestamp to the message
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            message = $"[{timestamp}] {message}";
            Console.WriteLine(message);
        }
    }
});

// Step 4: Run the orchestration
try
{
    var result = await service.RunAsync(
        new OrchestrationRequest(prompt),
        CancellationToken.None
    );

    // Close the event channel
    service.Events.Writer.TryComplete();
    await eventTask;

    Console.WriteLine(new string('─', 60));
    Console.WriteLine($"\n✅ Orchestration Complete!");
    Console.WriteLine($"Summary: {result.Summary}");

    // Step 5: Show the generated files
    var workspacePath = result.WorkspacePath;
    Console.WriteLine($"Workspace: {workspacePath}\n");

    var files = Directory.GetFiles(workspacePath, "*.cs", SearchOption.TopDirectoryOnly)
        .Concat(Directory.GetFiles(workspacePath, "*.csproj", SearchOption.TopDirectoryOnly))
        .Select(Path.GetFileName)
        .ToList();

    Console.WriteLine("📁 Generated files:");
    foreach (var file in files)
    {
        Console.WriteLine($"  • {file}");
    }

    // Step 6: Show how to run the app
    var projectFile = files.FirstOrDefault(f => f?.EndsWith(".csproj") == true);
    if (projectFile is not null)
    {
        Console.WriteLine($"\n📋 To run the generated app:\n");
        Console.WriteLine($"  dotnet run --project \"{workspacePath}\\{projectFile}\"\n");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ Error: {ex.Message}\n");
    Environment.Exit(1);
}
