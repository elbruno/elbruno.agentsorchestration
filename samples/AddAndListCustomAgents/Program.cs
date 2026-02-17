using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.AgentRepository.Services;

namespace AddAndListCustomAgents;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🤖 Dynamic Agent Loading Sample");
        Console.WriteLine("================================\n");

        var staticStore = new AgentConfigurationStore();
        Console.WriteLine($"✅ Loaded {staticStore.GetAll().Count} static agents");

        var loader = new AwesomeCopilotAgentLoader();
        var manager = new DynamicAgentManager(staticStore, loader);

        Console.WriteLine("\n📦 Loading WinForms Expert agent...");
        try
        {
            var winFormsAgent = await manager.LoadAndRegisterAgentAsync("WinFormsExpert");
            Console.WriteLine($"✅ Loaded: {winFormsAgent.Name}");
            Console.WriteLine($"   Icon: {winFormsAgent.Icon}");
            Console.WriteLine($"   Instructions preview: {winFormsAgent.Instructions[..Math.Min(100, winFormsAgent.Instructions.Length)]}...");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed: {ex.Message}");
        }

        Console.WriteLine("\n📦 Loading Security Reviewer agent...");
        try
        {
            var secAgent = await manager.LoadAndRegisterAgentAsync("se-security-reviewer");
            Console.WriteLine($"✅ Loaded: {secAgent.Name}");
            Console.WriteLine($"   Icon: {secAgent.Icon}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed: {ex.Message}");
        }

        Console.WriteLine("\n📋 All available agents:");
        var allAgents = manager.GetAllAgents();
        foreach (var agent in allAgents)
        {
            var isDynamic = manager.GetDynamicAgentNames().Contains(agent.Name);
            var marker = isDynamic ? "🔌" : "⚙️";
            Console.WriteLine($"   {marker} {agent.Icon} {agent.Name}");
        }

        Console.WriteLine($"\n✅ Total: {allAgents.Count} ({staticStore.GetAll().Count} static, {manager.GetDynamicAgentNames().Count} dynamic)");
        Console.WriteLine("\n✨ Sample completed!");
    }
}
