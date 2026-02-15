using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.Orchestration;
using ElBruno.AgentsOrchestration.Workspace;

namespace AgentsOrchestration.Core.Tests;

/// <summary>
/// Tests covering security, scalability, and edge-case scenarios
/// across all three library packages.
/// </summary>
public class SecurityAndEdgeCaseTests
{
    // ──────────────────────────────────────
    // Abstractions: AgentConfigurationStore
    // ──────────────────────────────────────

    [Fact]
    public void ConfigurationStore_GetAll_ReturnsAllSixRoles()
    {
        var store = new AgentConfigurationStore();

        var all = store.GetAll();

        Assert.Equal(11, all.Count);
        Assert.Contains(all, c => c.Role == AgentRole.Orchestrator);
        Assert.Contains(all, c => c.Role == AgentRole.Planner);
        Assert.Contains(all, c => c.Role == AgentRole.Coder);
        Assert.Contains(all, c => c.Role == AgentRole.Designer);
        Assert.Contains(all, c => c.Role == AgentRole.Researcher);
        Assert.Contains(all, c => c.Role == AgentRole.Fixer);
        Assert.Contains(all, c => c.Role == AgentRole.BuildReviewer);
        Assert.Contains(all, c => c.Role == AgentRole.SecurityExpert);
        Assert.Contains(all, c => c.Role == AgentRole.TestingExpert);
        Assert.Contains(all, c => c.Role == AgentRole.DocumentationExpert);
        Assert.Contains(all, c => c.Role == AgentRole.SoftwareArchitect);
    }

    [Fact]
    public void ConfigurationStore_Update_PersistsChanges()
    {
        var store = new AgentConfigurationStore();
        var original = store.Get(AgentRole.Coder);

        store.Update(original with { Model = "custom-model", Instructions = "custom instructions" });

        var updated = store.Get(AgentRole.Coder);
        Assert.Equal("custom-model", updated.Model);
        Assert.Equal("custom instructions", updated.Instructions);
    }

    [Fact]
    public void ConfigurationStore_Get_ThrowsForInvalidRole()
    {
        var store = new AgentConfigurationStore();

        Assert.Throws<KeyNotFoundException>(() => store.Get((AgentRole)999));
    }

    [Fact]
    public void ConfigurationStore_WithCustomInstructions_UsesProvidedValues()
    {
        var custom = new Dictionary<AgentRole, string>
        {
            [AgentRole.Orchestrator] = "custom-o",
            [AgentRole.Planner] = "custom-p",
            [AgentRole.Coder] = "custom-c",
            [AgentRole.Designer] = "custom-d",
            [AgentRole.Researcher] = "custom-r",
            [AgentRole.Fixer] = "custom-f",
            [AgentRole.BuildReviewer] = "custom-br",
            [AgentRole.SecurityExpert] = "custom-se",
            [AgentRole.TestingExpert] = "custom-te",
            [AgentRole.DocumentationExpert] = "custom-de",
            [AgentRole.SoftwareArchitect] = "custom-sa"
        };

        var store = new AgentConfigurationStore(custom);

        Assert.Equal("custom-c", store.Get(AgentRole.Coder).Instructions);
        Assert.Equal("custom-p", store.Get(AgentRole.Planner).Instructions);
    }

    // ──────────────────────────────────────
    // Abstractions: AgentFactory & Session
    // ──────────────────────────────────────

    [Fact]
    public void AgentFactory_CreateSession_ReturnsConfiguredSession()
    {
        var store = new AgentConfigurationStore();
        var client = new TemplateAgentClient();
        var factory = new AgentFactory(store, client);

        var session = factory.CreateSession(AgentRole.Planner);

        Assert.Equal(AgentRole.Planner, session.Configuration.Role);
        Assert.Equal("Planner", session.Configuration.Name);
    }

    [Fact]
    public async Task AgentSession_RunAsync_DelegatesToClient()
    {
        var store = new AgentConfigurationStore();
        var client = new TemplateAgentClient();
        var factory = new AgentFactory(store, client);

        var session = factory.CreateSession(AgentRole.Designer);
        var result = await session.RunAsync("test prompt", "/workspace", CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(result));
        Assert.Contains("Styles generated for", result);
    }

    // ──────────────────────────────────────
    // Abstractions: InstructionLoader
    // ──────────────────────────────────────

    [Fact]
    public void InstructionLoader_ThrowsWhenDirectoryDoesNotExist()
    {
        var nonExistent = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        Assert.Throws<DirectoryNotFoundException>(() =>
            InstructionLoader.LoadFromDirectory(nonExistent));
    }

    [Fact]
    public void InstructionLoader_ReturnsEmptyStrings_WhenFilesDoNotExist()
    {
        var emptyDir = Path.Combine(Path.GetTempPath(), $"instr-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(emptyDir);
        try
        {
            var instructions = InstructionLoader.LoadFromDirectory(emptyDir);

            Assert.Equal(11, instructions.Count);
            Assert.All(instructions.Values, v => Assert.Equal(string.Empty, v));
        }
        finally
        {
            Directory.Delete(emptyDir, true);
        }
    }

    [Fact]
    public void InstructionLoader_LoadsFromDirectory()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"instr-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "planner.md"), "Plan things carefully.");
            File.WriteAllText(Path.Combine(dir, "coder.md"), "Write clean code.");

            var instructions = InstructionLoader.LoadFromDirectory(dir);

            Assert.Equal("Plan things carefully.", instructions[AgentRole.Planner]);
            Assert.Equal("Write clean code.", instructions[AgentRole.Coder]);
            Assert.Equal(string.Empty, instructions[AgentRole.Designer]);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    // ──────────────────────────────────────
    // Orchestration: ParsePlan edge cases
    // ──────────────────────────────────────

    [Fact]
    public void ParsePlan_ReturnsDefaultPhase_WhenOutputIsEmpty()
    {
        var plan = OrchestrationService.ParsePlan("", "test prompt");

        Assert.Single(plan.Phases);
        Assert.Equal("Implementation", plan.Phases.First().Name);
        Assert.Equal(3, plan.Phases.First().Tasks.Count);
    }

    [Fact]
    public void ParsePlan_ReturnsDefaultPhase_WhenNoPhaseHeaders()
    {
        var output = "This is just some text without any phases.";

        var plan = OrchestrationService.ParsePlan(output, "test prompt");

        Assert.Single(plan.Phases);
        Assert.Equal("Implementation", plan.Phases.First().Name);
    }

    [Fact]
    public void ParsePlan_ParsesMultiplePhases()
    {
        var output = """
            ## Phase 1: Setup
            - Task: Create project | Agent: Coder | File: app.csproj
            ## Phase 2: Implementation
            - Task: Write logic | Agent: Coder | File: Program.cs
            - Task: Add styles | Agent: Designer | File: styles.css
            """;

        var plan = OrchestrationService.ParsePlan(output, "build app");

        Assert.Equal(2, plan.Phases.Count);
        Assert.Single(plan.Phases.ElementAt(0).Tasks);
        Assert.Equal(2, plan.Phases.ElementAt(1).Tasks.Count);
    }

    [Fact]
    public void ParsePlan_SkipsInvalidTaskLines()
    {
        var output = """
            ## Phase 1: Setup
            - Task: Valid task | Agent: Coder | File: app.cs
            - Task: No pipe separator
            - Not a task line
            """;

        var plan = OrchestrationService.ParsePlan(output, "test");

        Assert.Single(plan.Phases);
        Assert.Single(plan.Phases.First().Tasks);
    }

    [Fact]
    public void ParsePlan_DefaultsToCoderRole_WhenAgentInvalid()
    {
        var output = """
            ## Phase 1: Build
            - Task: Do something | Agent: UnknownAgent | File: output.cs
            """;

        var plan = OrchestrationService.ParsePlan(output, "test");

        Assert.Equal(AgentRole.Coder, plan.Phases.First().Tasks.First().AssignedRole);
    }

    // ──────────────────────────────────────
    // Orchestration: Security & Validation
    // ──────────────────────────────────────

    [Fact]
    public async Task OrchestrationService_RunAsync_ValidatesPrompt()
    {
        var store = new AgentConfigurationStore();
        var factory = new AgentFactory(store, new TemplateAgentClient());
        var root = Path.Combine(Path.GetTempPath(), $"sec-test-{Guid.NewGuid():N}");
        try
        {
            var workspace = new WorkspaceManager(root);
            var service = new OrchestrationService(factory, workspace);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.RunAsync(new OrchestrationRequest(""), CancellationToken.None));

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.RunAsync(new OrchestrationRequest("   "), CancellationToken.None));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void OrchestrationService_ClampsMaxFixAttempts()
    {
        var store = new AgentConfigurationStore();
        var factory = new AgentFactory(store, new TemplateAgentClient());
        var root = Path.Combine(Path.GetTempPath(), $"clamp-test-{Guid.NewGuid():N}");
        try
        {
            var workspace = new WorkspaceManager(root);

            // Should not throw — clamped to 10
            var service = new OrchestrationService(factory, workspace, maxFixAttempts: 100);

            // Should not throw — clamped to 0
            var service2 = new OrchestrationService(factory, workspace, maxFixAttempts: -5);

            // Verify both created successfully
            Assert.NotNull(service);
            Assert.NotNull(service2);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task OrchestrationService_RunAsync_CanBeCancelled()
    {
        var store = new AgentConfigurationStore();
        var client = new SlowAgentClient();
        var factory = new AgentFactory(store, client);
        var root = Path.Combine(Path.GetTempPath(), $"cancel-test-{Guid.NewGuid():N}");
        try
        {
            var workspace = new WorkspaceManager(root);
            var service = new OrchestrationService(factory, workspace);

            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                service.RunAsync(new OrchestrationRequest("build app"), cts.Token));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    // ──────────────────────────────────────
    // Core: WorkspaceManager security
    // ──────────────────────────────────────

    [Fact]
    public void WorkspaceManager_CreateWorkspace_SanitizesPromptSlug()
    {
        var root = Path.Combine(Path.GetTempPath(), $"slug-test-{Guid.NewGuid():N}");
        try
        {
            var manager = new WorkspaceManager(root);

            // Special characters should be stripped
            var path1 = manager.CreateWorkspace("../../etc/passwd");
            Assert.StartsWith(Path.GetFullPath(root), Path.GetFullPath(path1));

            // Empty/whitespace prompt should use default slug
            var manager2 = new WorkspaceManager(root);
            var path2 = manager2.CreateWorkspace("   ");
            Assert.StartsWith(Path.GetFullPath(root), Path.GetFullPath(path2));

            // Null prompt should not throw
            var manager3 = new WorkspaceManager(root);
            var path3 = manager3.CreateWorkspace(null!);
            Assert.StartsWith(Path.GetFullPath(root), Path.GetFullPath(path3));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void WorkspaceManager_ReadFile_RejectsPathTraversal()
    {
        var root = Path.Combine(Path.GetTempPath(), $"traversal-test-{Guid.NewGuid():N}");
        try
        {
            var manager = new WorkspaceManager(root);
            manager.CreateWorkspace("test");

            // Attempting to read outside workspace should return empty
            var result = manager.ReadFile("../../etc/passwd");
            Assert.Equal(string.Empty, result);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void WorkspaceManager_ListFiles_ReturnsEmpty_BeforeCreate()
    {
        var root = Path.Combine(Path.GetTempPath(), $"list-test-{Guid.NewGuid():N}");
        try
        {
            var manager = new WorkspaceManager(root);

            var files = manager.ListFiles();

            Assert.Empty(files);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    // ──────────────────────────────────────
    // Core: TemplateAgentClient all roles
    // ──────────────────────────────────────

    [Fact]
    public async Task TemplateAgentClient_DesignerRole_ReturnsCss()
    {
        var client = new TemplateAgentClient();

        var result = await client.RunAsync(AgentRole.Designer, "Create styles for app", "/ws", CancellationToken.None);

        Assert.Contains(":root", result);
        Assert.Contains("--primary", result);
        Assert.Contains("body", result);
    }

    [Fact]
    public async Task TemplateAgentClient_OrchestratorRole_ReturnsVerification()
    {
        var client = new TemplateAgentClient();

        var result = await client.RunAsync(AgentRole.Orchestrator, "verify workspace", "/ws", CancellationToken.None);

        Assert.Contains("Orchestration verified", result);
        Assert.Contains("/ws", result);
    }

    [Fact]
    public async Task TemplateAgentClient_ResearcherRole_ReturnsResearchSummary()
    {
        var client = new TemplateAgentClient();

        var result = await client.RunAsync(AgentRole.Researcher, "research HttpClient retry patterns", "/ws", CancellationToken.None);

        Assert.Contains("# Research Summary", result);
        Assert.Contains("Sources", result);
        Assert.Contains("Recommendations", result);
    }

    // ──────────────────────────────────────
    // Helper: Slow agent client for cancellation tests
    // ──────────────────────────────────────

    private sealed class SlowAgentClient : IAgentClient
    {
        public async Task<string> RunAsync(AgentRole role, string prompt, string workspacePath, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            return "Should not reach here";
        }
    }
}
