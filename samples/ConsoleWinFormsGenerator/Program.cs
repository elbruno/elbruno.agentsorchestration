using ElBruno.AgentsOrchestration.Core.Orchestration;
using ElBruno.AgentsOrchestration.Orchestration;
using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.AgentRepository.Services;

namespace ConsoleWinFormsGenerator;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🪟 WinForms Application Generator with Dynamic Agent");
        Console.WriteLine("====================================================\n");

        // Initialize static agents
        var staticStore = new AgentConfigurationStore();
        Console.WriteLine($"✅ Loaded {staticStore.GetAll().Count} core agents\n");

        // Load WinForms Expert agent dynamically
        Console.WriteLine("📦 Loading WinForms Expert agent...");
        var loader = new AwesomeCopilotAgentLoader();
        var manager = new DynamicAgentManager(staticStore, loader);

        AgentConfiguration? winFormsAgent = null;
        try
        {
            winFormsAgent = await manager.LoadAndRegisterAgentAsync("WinFormsExpert");
            Console.WriteLine($"✅ Loaded: {winFormsAgent.Name} ({winFormsAgent.Icon})\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Could not load WinForms Expert: {ex.Message}");
            Console.WriteLine("   Continuing with standard agents...\n");
        }

        // Create orchestration service with dynamic agents
        var service = OrchestrationServiceFactory.Create();

        // Generate a WinForms application
        var prompt = @"Create a professional WinForms application with the following features:
1. Main form with a title 'Todo Manager'
2. ListView control to display tasks
3. Add, Edit, and Delete buttons
4. Task completion checkbox column
5. Save/Load functionality using JSON
6. Proper MVVM pattern with separate data models
7. Input validation
8. Error handling with message boxes

The application should be well-structured with proper separation of concerns.";

        Console.WriteLine("🔨 Starting orchestration to generate WinForms application...\n");
        Console.WriteLine($"📝 Prompt: {prompt.Substring(0, 80)}...\n");

        try
        {
            var result = await service.RunAsync(
                new OrchestrationRequest(prompt),
                CancellationToken.None
            );

            Console.WriteLine("\n✨ Orchestration completed!\n");
            Console.WriteLine($"📁 Workspace: {result.WorkspacePath}");
            Console.WriteLine($"� Generated {result.TaskResults.Count} tasks");
            Console.WriteLine($"\n📄 Summary:\n{result.Summary}");

            Console.WriteLine("\n🎉 Your WinForms application is ready!");
            Console.WriteLine($"📦 To run it:");
            Console.WriteLine($"   cd {result.WorkspacePath}");
            Console.WriteLine($"   dotnet run");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error during orchestration: {ex.Message}");
            Console.WriteLine($"Details: {ex}");
        }

        Console.WriteLine("\n✨ Sample completed!");
    }
}
