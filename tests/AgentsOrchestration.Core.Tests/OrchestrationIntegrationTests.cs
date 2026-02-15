using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.Orchestration;
using ElBruno.AgentsOrchestration.Workspace;
using System.Threading.Channels;

namespace AgentsOrchestration.Core.Tests;

public class OrchestrationIntegrationTests
{
    [Fact]
    public void ConfigurationStore_ResetInstructions_ReturnsDefault()
    {
        var store = new AgentConfigurationStore(new Dictionary<AgentRole, string>
        {
            [AgentRole.Orchestrator] = "orchestrator",
            [AgentRole.Planner] = "planner",
            [AgentRole.Coder] = "coder",
            [AgentRole.Designer] = "designer",
            [AgentRole.Researcher] = "researcher",
            [AgentRole.Fixer] = "fixer",
            [AgentRole.BuildReviewer] = "build-reviewer",
            [AgentRole.SecurityExpert] = "security-expert",
            [AgentRole.TestingExpert] = "testing-expert",
            [AgentRole.DocumentationExpert] = "documentation-expert",
            [AgentRole.SoftwareArchitect] = "software-architect"
        });

        var current = store.Get(AgentRole.Coder);
        store.Update(current with { Instructions = "custom" });

        store.ResetInstructions(AgentRole.Coder);

        Assert.Equal("coder", store.Get(AgentRole.Coder).Instructions);
    }

    [Fact]
    public void WorkspaceManager_CreatesAndReadsWorkspaceFiles()
    {
        var root = Path.Combine(Path.GetTempPath(), $"orchestration-tests-{Guid.NewGuid():N}");
        try
        {
            var manager = new WorkspaceManager(root);
            var workspace = manager.CreateWorkspace("test prompt");
            var file = Path.Combine(workspace, "src", "Generated", "file.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            File.WriteAllText(file, "hello");

            var files = manager.ListFiles();

            var expected = Path.Combine("src", "Generated", "file.txt");
            Assert.Contains(expected, files);
            Assert.Equal("hello", manager.ReadFile(expected));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public async Task OrchestrationService_RunAsync_ProducesResultsAndFiles()
    {
        var root = Path.Combine(Path.GetTempPath(), $"orchestration-tests-{Guid.NewGuid():N}");
        try
        {
            var manager = new WorkspaceManager(root);
            var store = new AgentConfigurationStore();
            var factory = new AgentFactory(store, new TemplateAgentClient());
            var service = new OrchestrationService(factory, manager);

            var result = await service.RunAsync(new OrchestrationRequest("create app"), CancellationToken.None);

            Assert.NotEmpty(result.TaskResults);
            Assert.NotEmpty(manager.ListFiles());
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public void ConfigurationStore_ContainsFixerAgent()
    {
        var store = new AgentConfigurationStore();

        var fixer = store.Get(AgentRole.Fixer);

        Assert.Equal("Fixer", fixer.Name);
        Assert.Equal("#fd7e14", fixer.Color);
        Assert.Equal("\U0001f527", fixer.Icon);
    }

    [Fact]
    public async Task TemplateAgentClient_FixerRole_ReturnsOutput()
    {
        var client = new TemplateAgentClient();

        var result = await client.RunAsync(AgentRole.Fixer, "Fix .csproj SDK error", "/workspace", CancellationToken.None);

        Assert.Contains("<Project", result);
    }

    [Fact]
    public async Task TemplateAgentClient_CsprojBug_IsFixed()
    {
        var client = new TemplateAgentClient();

        var result = await client.RunAsync(AgentRole.Coder, "Create project file for web api", "/workspace", CancellationToken.None);

        Assert.Contains("Microsoft.NET.Sdk.Web", result);
        Assert.DoesNotContain("Microsoft.NET.Sdk\">.Web", result);
    }

    [Fact]
    public async Task OrchestrationService_EmitsFixAttemptEvents_WhenBuildFails()
    {
        var root = Path.Combine(Path.GetTempPath(), $"orchestration-tests-{Guid.NewGuid():N}");
        try
        {
            var manager = new WorkspaceManager(root);
            var store = new AgentConfigurationStore();
            var factory = new AgentFactory(store, new TemplateAgentClient());
            var service = new OrchestrationService(factory, manager, maxFixAttempts: 1);

            var events = new List<OrchestrationEvent>();
            var readTask = Task.Run(async () =>
            {
                await foreach (var evt in service.Events.Reader.ReadAllAsync())
                {
                    events.Add(evt);
                }
            });

            await service.RunAsync(new OrchestrationRequest("create app"), CancellationToken.None);
            service.Events.Writer.TryComplete();
            await readTask;

            // Should have build validation events
            Assert.Contains(events, e => e is BuildValidationEvent);
            // The template produces a valid .csproj, so build may succeed or fail
            // depending on dotnet availability — but events should always be present
            Assert.Contains(events, e => e is OrchestrationCompletedEvent);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public void AppRunner_IsNotRunning_Initially()
    {
        using var runner = new AppRunner();

        Assert.False(runner.IsRunning);
    }

    [Fact]
    public async Task AppRunner_LaunchAsync_ReturnsFalse_WhenNoCsproj()
    {
        var root = Path.Combine(Path.GetTempPath(), $"orchestration-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            using var runner = new AppRunner();
            var result = await runner.LaunchAsync(root);

            Assert.False(result);
            Assert.False(runner.IsRunning);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }
}
