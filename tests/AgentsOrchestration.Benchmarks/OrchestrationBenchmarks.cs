using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.Workspace;

namespace AgentsOrchestration.Benchmarks;

/// <summary>
/// Benchmarks for critical orchestration paths: agent instantiation and workspace operations.
/// Run with: dotnet run -c Release --project tests/AgentsOrchestration.Benchmarks
/// </summary>
public class OrchestrationBenchmarks
{
    private AgentConfigurationStore _store = null!;
    private TemplateAgentClient _client = null!;
    private AgentFactory _factory = null!;
    private WorkspaceManager _workspace = null!;

    [GlobalSetup]
    public void Setup()
    {
        _store = new AgentConfigurationStore();
        _client = new TemplateAgentClient();
        _factory = new AgentFactory(_store, _client);
        
        var tempPath = Path.Combine(Path.GetTempPath(), $"benchmark-{Guid.NewGuid():N}");
        _workspace = new WorkspaceManager(tempPath);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        foreach (var dir in Directory.GetDirectories(Path.GetTempPath(), "benchmark-*"))
        {
            try { Directory.Delete(dir, true); } catch { }
        }
    }

    [Benchmark]
    public void AgentSession_Creation()
    {
        var session = _factory.CreateSession(AgentRole.Coder);
    }

    [Benchmark]
    public async Task AgentSession_RunAsync()
    {
        var session = _factory.CreateSession(AgentRole.Planner);
        await session.RunAsync("Create a simple web app", "/workspace", CancellationToken.None);
    }

    [Benchmark]
    public void ConfigurationStore_GetAllRoles()
    {
        var all = _store.GetAll();
    }

    [Benchmark]
    public void ConfigurationStore_Update()
    {
        var config = _store.Get(AgentRole.Coder);
        _store.Update(config with { Model = "benchmark-model" });
    }

    [Benchmark]
    public void Workspace_CreateWorkspace()
    {
        _workspace.CreateWorkspace("benchmark test prompt");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<OrchestrationBenchmarks>();
    }
}
