using System.Diagnostics;
using ElBruno.AgentsOrchestration.Core.Orchestration;
using ElBruno.AgentsOrchestration.Orchestration;
using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.AgentRepository.Services;

Console.WriteLine("🪟 WinForms Application Generator with Dynamic Agent");
Console.WriteLine("====================================================\n");

// Initialize static agents
var staticStore = new AgentConfigurationStore();
Console.WriteLine($"✅ Loaded {staticStore.GetAll().Count} core agents\n");

// Load WinForms Expert agent dynamically
Console.WriteLine("📦 Loading WinForms Expert agent...");
var loader = new AwesomeCopilotAgentLoader();
var manager = new DynamicAgentManager(staticStore, loader);

try
{
    var winFormsAgent = await manager.LoadAndRegisterAgentAsync("WinFormsExpert");
    Console.WriteLine($"✅ Loaded: {winFormsAgent.Name} ({winFormsAgent.Icon})\n");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️  Could not load WinForms Expert: {ex.Message}");
    Console.WriteLine("   Continuing with standard agents...\n");
}

// Create orchestration service
var service = OrchestrationServiceFactory.Create();

// Simple WinForms application prompt
var prompt = @"Create a simple Windows Forms Counter application with:
1. A main form titled 'Counter App'
2. A label displaying the counter value (starting at 0)
3. Two buttons: 'Increment' and 'Reset'
4. Button click handlers that update the counter label
5. Clean and simple code structure

Keep the implementation minimal and straightforward.";

Console.WriteLine("🔨 Starting orchestration to generate WinForms app...\n");

try
{
    var result = await service.RunAsync(
        new OrchestrationRequest(prompt),
        CancellationToken.None
    );

    Console.WriteLine("\n✨ Orchestration completed!\n");
    Console.WriteLine($"📁 Workspace: {result.WorkspacePath}");
    Console.WriteLine($"📋 Generated {result.TaskResults.Count} tasks\n");

    // Validate the generated code builds
    Console.WriteLine("🔍 Validating generated code build...");
    var buildResult = await ValidateBuildAsync(result.WorkspacePath);
    
    if (buildResult.Success)
    {
        Console.WriteLine("✅ Code builds successfully!\n");
        Console.WriteLine("🎉 Your WinForms application is ready!");
        Console.WriteLine($"📦 To run it:\n");
        Console.WriteLine($"   cd {result.WorkspacePath}");
        Console.WriteLine($"   dotnet run\n");
    }
    else
    {
        Console.WriteLine($"⚠️  Build validation found issues:\n");
        Console.WriteLine("Build Errors:");
        foreach (var error in buildResult.Errors.Take(5))
        {
            Console.WriteLine($"  • {error}");
        }
        Console.WriteLine("\n💡 The orchestration pipeline includes build validation internally.");
        Console.WriteLine("   Check the workspace for generated files.\n");
    }

    Console.WriteLine($"📄 Summary:\n{result.Summary}");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error during orchestration: {ex.Message}");
}

Console.WriteLine("\n✨ Sample completed!");

// Local function
async Task<BuildValidationResult> ValidateBuildAsync(string workspacePath)
{
    try
    {
        var projectFile = Directory.GetFiles(workspacePath, "*.csproj", SearchOption.TopDirectoryOnly)
            .FirstOrDefault();

        if (projectFile == null)
            return new(false, new[] { "No .csproj file found in workspace" });

        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build \"{projectFile}\"",
            WorkingDirectory = workspacePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process == null)
            return new(false, new[] { "Failed to start build process" });

        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        await Task.Run(() => process.WaitForExit());

        if (process.ExitCode == 0)
            return new(true, Array.Empty<string>());

        var errors = new List<string>();
        var allOutput = stdout + "\n" + stderr;
        foreach (var line in allOutput.Split('\n'))
        {
            if (line.Contains("error CS") && !string.IsNullOrWhiteSpace(line))
            {
                var match = line.Substring(line.IndexOf("error CS"));
                errors.Add(match.Trim());
            }
        }

        return new(false, errors.Count > 0 ? errors.ToArray() : new[] { "Build failed" });
    }
    catch (Exception ex)
    {
        return new(false, new[] { $"Validation error: {ex.Message}" });
    }
}

record BuildValidationResult(bool Success, string[] Errors);
