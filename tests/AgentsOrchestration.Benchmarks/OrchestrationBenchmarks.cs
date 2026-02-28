using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.Orchestration;
using ElBruno.AgentsOrchestration.Workspace;

namespace AgentsOrchestration.Benchmarks;

/// <summary>
/// Benchmarks for critical orchestration paths: agent instantiation, event processing, and plan parsing.
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
        var tempPath = Path.Combine(Path.GetTempPath(), $"benchmark-*");
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
    public void ParsePlan_SimplePhases()
    {
        var planOutput = """
            ## Phase 1: Setup
            - Task: Create project | Agent: Coder | File: app.csproj
            ## Phase 2: Implementation
            - Task: Write logic | Agent: Coder | File: Program.cs
            - Task: Add styles | Agent: Designer | File: styles.css
            """;

        OrchestrationService.ParsePlan(planOutput, "build app");
    }

    [Benchmark]
    public void ParsePlan_ComplexPhases()
    {
        var planOutput = """
            ## Phase 1: Setup
            - Task: Create project | Agent: Coder | File: app.csproj
            - Task: Setup dependencies | Agent: Coder | File: packages.json
            ## Phase 2: Core Implementation
            - Task: Write API | Agent: Coder | File: api.cs
            - Task: Write database layer | Agent: Coder | File: data.cs
            - Task: Write business logic | Agent: Coder | File: logic.cs
            ## Phase 3: UI
            - Task: Create layout | Agent: Designer | File: index.html
            - Task: Add styles | Agent: Designer | File: styles.css
            - Task: Add interactions | Agent: Coder | File: app.js
            ## Phase 4: Testing
            - Task: Write unit tests | Agent: TestingExpert | File: tests.cs
            - Task: Write integration tests | Agent: TestingExpert | File: integration.cs
            """;

        OrchestrationService.ParsePlan(planOutput, "build comprehensive app");
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
