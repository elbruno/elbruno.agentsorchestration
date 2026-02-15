using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.Orchestration;

namespace AgentsOrchestration.Core.Tests;

/// <summary>
/// Tests for Researcher agent, research models, flow tracing, and inter-agent communication features.
/// </summary>
public class ResearcherAgentTests
{
    // ──────────────────────────────────────
    // Research Models
    // ──────────────────────────────────────

    [Fact]
    public void ResearchRequest_CreatesWithAllProperties()
    {
        var request = new ResearchRequest(
            RequestingAgent: AgentRole.Coder,
            Query: "HttpClient retry patterns",
            Context: "Building API client",
            Scope: ResearchScope.All,
            MaxResults: 10
        );

        Assert.Equal(AgentRole.Coder, request.RequestingAgent);
        Assert.Equal("HttpClient retry patterns", request.Query);
        Assert.Equal("Building API client", request.Context);
        Assert.Equal(ResearchScope.All, request.Scope);
        Assert.Equal(10, request.MaxResults);
    }

    [Fact]
    public void ResearchRequest_UsesDefaultMaxResults()
    {
        var request = new ResearchRequest(
            RequestingAgent: AgentRole.Designer,
            Query: "CSS Grid patterns",
            Context: "Dashboard layout",
            Scope: ResearchScope.WebSearch
        );

        Assert.Equal(5, request.MaxResults); // Default value
    }

    [Fact]
    public void ResearchSource_CreatesWithAllProperties()
    {
        var source = new ResearchSource(
            Title: "Polly Documentation",
            Url: "https://github.com/App-vNext/Polly",
            Excerpt: "Resilience and transient-fault-handling library",
            SourceType: "library-docs"
        );

        Assert.Equal("Polly Documentation", source.Title);
        Assert.Equal("https://github.com/App-vNext/Polly", source.Url);
        Assert.Equal("Resilience and transient-fault-handling library", source.Excerpt);
        Assert.Equal("library-docs", source.SourceType);
    }

    [Fact]
    public void ResearchResponse_IncludesRequestAndResults()
    {
        var request = new ResearchRequest(
            AgentRole.Fixer,
            "CS0246 error solution",
            "Missing namespace",
            ResearchScope.Documentation
        );

        var sources = new List<ResearchSource>
        {
            new("Microsoft Learn", "https://learn.microsoft.com", "Add using directive", "docs"),
            new("Stack Overflow", "https://stackoverflow.com", "Check references", "web")
        };

        var response = new ResearchResponse(
            Request: request,
            Summary: "Found 2 solutions for namespace errors",
            Sources: sources,
            CompletedAt: DateTimeOffset.UtcNow
        );

        Assert.Equal(request, response.Request);
        Assert.Equal("Found 2 solutions for namespace errors", response.Summary);
        Assert.Equal(2, response.Sources.Count);
        Assert.Contains(response.Sources, s => s.Title == "Microsoft Learn");
    }

    [Fact]
    public void ResearchScope_HasAllExpectedValues()
    {
        var values = Enum.GetValues<ResearchScope>();

        Assert.Contains(ResearchScope.WebSearch, values);
        Assert.Contains(ResearchScope.Documentation, values);
        Assert.Contains(ResearchScope.CodeExamples, values);
        Assert.Contains(ResearchScope.BestPractices, values);
        Assert.Contains(ResearchScope.All, values);
        Assert.Equal(5, values.Length);
    }

    // ──────────────────────────────────────
    // AgentToolConfiguration
    // ──────────────────────────────────────

    [Fact]
    public void AgentToolConfiguration_DefaultsToNoToolsEnabled()
    {
        var config = new AgentToolConfiguration();

        Assert.False(config.WebSearchEnabled);
        Assert.False(config.MicrosoftLearnMcpEnabled);
        Assert.False(config.Context7McpEnabled);
        Assert.Null(config.CustomMcpServers);
    }

    [Fact]
    public void AgentToolConfiguration_CanEnableAllTools()
    {
        var config = new AgentToolConfiguration(
            WebSearchEnabled: true,
            MicrosoftLearnMcpEnabled: true,
            Context7McpEnabled: true,
            CustomMcpServers: new[] { "https://custom.mcp" }
        );

        Assert.True(config.WebSearchEnabled);
        Assert.True(config.MicrosoftLearnMcpEnabled);
        Assert.True(config.Context7McpEnabled);
        Assert.NotNull(config.CustomMcpServers);
        Assert.Single(config.CustomMcpServers);
    }

    [Fact]
    public void AgentConfiguration_AcceptsToolConfiguration()
    {
        var tools = new AgentToolConfiguration(WebSearchEnabled: true);
        var config = new AgentConfiguration(
            Role: AgentRole.Researcher,
            Name: "Test Researcher",
            Model: "gpt-4",
            Instructions: "Research things",
            Color: "#6610f2",
            Icon: "🔍",
            Tools: tools
        );

        Assert.NotNull(config.Tools);
        Assert.True(config.Tools.WebSearchEnabled);
    }

    [Fact]
    public void AgentConfiguration_ToolsAreOptional()
    {
        var config = new AgentConfiguration(
            Role: AgentRole.Orchestrator,
            Name: "Orchestrator",
            Model: "gpt-4",
            Instructions: "Coordinate agents",
            Color: "#6c757d",
            Icon: "🧭"
        );

        Assert.Null(config.Tools);
    }

    [Fact]
    public void Researcher_ConfigurationHasToolsEnabled()
    {
        var store = new AgentConfigurationStore();
        var researcher = store.Get(AgentRole.Researcher);

        Assert.NotNull(researcher.Tools);
        Assert.True(researcher.Tools.WebSearchEnabled);
        Assert.True(researcher.Tools.MicrosoftLearnMcpEnabled);
        Assert.True(researcher.Tools.Context7McpEnabled);
    }

    [Fact]
    public void NonResearcher_ConfigurationsHaveNoTools()
    {
        var store = new AgentConfigurationStore();

        Assert.Null(store.Get(AgentRole.Orchestrator).Tools);
        Assert.Null(store.Get(AgentRole.Planner).Tools);
        Assert.Null(store.Get(AgentRole.Coder).Tools);
        Assert.Null(store.Get(AgentRole.Designer).Tools);
        Assert.Null(store.Get(AgentRole.Fixer).Tools);
        Assert.Null(store.Get(AgentRole.BuildReviewer).Tools);
    }

    // ──────────────────────────────────────
    // AgentCallGraph
    // ──────────────────────────────────────

    [Fact]
    public void AgentCallGraph_StartsEmpty()
    {
        var graph = new AgentCallGraph();

        Assert.Empty(graph.Calls);
    }

    [Fact]
    public void AgentCallGraph_RecordsCall()
    {
        var graph = new AgentCallGraph();
        var timestamp = DateTimeOffset.UtcNow;

        graph.RecordCall(
            AgentRole.Orchestrator,
            AgentRole.Researcher,
            "Research retry patterns",
            timestamp,
            TimeSpan.FromSeconds(2.5)
        );

        Assert.Single(graph.Calls);
        var call = graph.Calls[0];
        Assert.Equal(AgentRole.Orchestrator, call.FromAgent);
        Assert.Equal(AgentRole.Researcher, call.ToAgent);
        Assert.Equal("Research retry patterns", call.Purpose);
        Assert.Equal(timestamp, call.Timestamp);
        Assert.Equal(TimeSpan.FromSeconds(2.5), call.Duration);
    }

    [Fact]
    public void AgentCallGraph_RecordsMultipleCalls()
    {
        var graph = new AgentCallGraph();

        graph.RecordCall(AgentRole.Orchestrator, AgentRole.Planner, "Plan", DateTimeOffset.UtcNow);
        graph.RecordCall(AgentRole.Orchestrator, AgentRole.Coder, "Code", DateTimeOffset.UtcNow);
        graph.RecordCall(AgentRole.Orchestrator, AgentRole.Researcher, "Research", DateTimeOffset.UtcNow);

        Assert.Equal(3, graph.Calls.Count);
    }

    [Fact]
    public void AgentCallGraph_RecordsRetryAttempts()
    {
        var graph = new AgentCallGraph();

        graph.RecordCall(AgentRole.Orchestrator, AgentRole.Fixer, "Fix build", DateTimeOffset.UtcNow, attemptNumber: 1);
        graph.RecordCall(AgentRole.Orchestrator, AgentRole.Fixer, "Fix build", DateTimeOffset.UtcNow, attemptNumber: 2);
        graph.RecordCall(AgentRole.Orchestrator, AgentRole.Fixer, "Fix build", DateTimeOffset.UtcNow, attemptNumber: 3);

        Assert.Equal(3, graph.Calls.Count);
        Assert.All(graph.Calls, call => Assert.NotNull(call.AttemptNumber));
        Assert.Equal(1, graph.Calls[0].AttemptNumber);
        Assert.Equal(2, graph.Calls[1].AttemptNumber);
        Assert.Equal(3, graph.Calls[2].AttemptNumber);
    }

    /*
    [Fact]
    public void AgentCallGraph_GeneratesMermaidDiagram()
    {
        var graph = new AgentCallGraph();
        graph.RecordCall(AgentRole.Orchestrator, AgentRole.Planner, "Create plan", DateTimeOffset.UtcNow);
        graph.RecordCall(AgentRole.Orchestrator, AgentRole.Coder, "Implement code", DateTimeOffset.UtcNow);
        graph.RecordCall(AgentRole.Orchestrator, AgentRole.Researcher, "Research API", DateTimeOffset.UtcNow);

        var mermaid = graph.GenerateMermaidDiagram();

        Assert.Contains("```mermaid", mermaid);
        Assert.Contains("sequenceDiagram", mermaid);
        Assert.Contains("participant O as Orchestrator", mermaid);
        Assert.Contains("participant R as Researcher", mermaid);
        Assert.Contains("O->>P: Create plan", mermaid);
        Assert.Contains("O->>C: Implement code", mermaid);
        Assert.Contains("O->>R: Research API", mermaid);
        Assert.Contains("```", mermaid);
    }

    [Fact]
    public void AgentCallGraph_MermaidDiagram_IncludesRetryNotes()
    {
        var graph = new AgentCallGraph();
        graph.RecordCall(AgentRole.Orchestrator, AgentRole.Fixer, "Fix error", DateTimeOffset.UtcNow, attemptNumber: 2);

        var mermaid = graph.GenerateMermaidDiagram();

        Assert.Contains("Note over F: Attempt 2 - Fix error", mermaid);
    }

    [Fact]
    public void AgentCallGraph_GeneratesJsonFlow()
    {
        var graph = new AgentCallGraph();
        graph.RecordCall(AgentRole.Orchestrator, AgentRole.Planner, "Plan", DateTimeOffset.UtcNow);
        graph.RecordCall(AgentRole.Orchestrator, AgentRole.Coder, "Code", DateTimeOffset.UtcNow);
        graph.RecordCall(AgentRole.Orchestrator, AgentRole.Coder, "Code", DateTimeOffset.UtcNow); // Duplicate

        var json = graph.GenerateJsonFlow();

        Assert.NotNull(json);
        Assert.Contains("\"nodes\"", json);
        Assert.Contains("\"edges\"", json);
        Assert.Contains("\"loops\"", json);
        Assert.Contains("\"orchestrator\"", json);
        Assert.Contains("\"planner\"", json);
        Assert.Contains("\"coder\"", json);
        // Should show coder called twice
        Assert.Contains("\"count\": 2", json);
    }

    [Fact]
    public void AgentCallGraph_JsonFlow_TracksLoops()
    {
        var graph = new AgentCallGraph();
        graph.RecordCall(AgentRole.Orchestrator, AgentRole.Fixer, "Fix 1", DateTimeOffset.UtcNow, attemptNumber: 1);
        graph.RecordCall(AgentRole.Orchestrator, AgentRole.Fixer, "Fix 2", DateTimeOffset.UtcNow, attemptNumber: 2);
        graph.RecordCall(AgentRole.Orchestrator, AgentRole.Fixer, "Fix 3", DateTimeOffset.UtcNow, attemptNumber: 3);

        var json = graph.GenerateJsonFlow();

        Assert.Contains("\"loops\"", json);
        Assert.Contains("\"fixer\"", json);
        Assert.Contains("\"iterations\": 3", json);
    }
    */

    // ──────────────────────────────────────
    // OrchestrationConfiguration
    // ──────────────────────────────────────

    [Fact]
    public void OrchestrationConfiguration_HasCorrectDefaults()
    {
        var config = new OrchestrationConfiguration();

        Assert.True(config.TracingEnabled);
        Assert.True(config.GenerateMermaidDiagram);
        Assert.True(config.GenerateJsonFlow);
        Assert.Equal(1000, config.MaxTrackedCalls);
        Assert.Equal(3, config.MaxRetryAttempts);
        Assert.Equal(3, config.ResearchTriggerThreshold);
    }

    [Fact]
    public void OrchestrationConfiguration_AllowsCustomization()
    {
        var config = new OrchestrationConfiguration(
            TracingEnabled: false,
            GenerateMermaidDiagram: false,
            GenerateJsonFlow: true,
            MaxTrackedCalls: 500,
            MaxRetryAttempts: 5,
            ResearchTriggerThreshold: 2
        );

        Assert.False(config.TracingEnabled);
        Assert.False(config.GenerateMermaidDiagram);
        Assert.True(config.GenerateJsonFlow);
        Assert.Equal(500, config.MaxTrackedCalls);
        Assert.Equal(5, config.MaxRetryAttempts);
        Assert.Equal(2, config.ResearchTriggerThreshold);
    }

    [Fact]
    public void OrchestrationConfiguration_SupportsWithExpressions()
    {
        var original = new OrchestrationConfiguration();
        var modified = original with { TracingEnabled = false, MaxRetryAttempts = 10 };

        Assert.True(original.TracingEnabled);
        Assert.False(modified.TracingEnabled);
        Assert.Equal(3, original.MaxRetryAttempts);
        Assert.Equal(10, modified.MaxRetryAttempts);
    }

    // ──────────────────────────────────────
    // Research Events
    // ──────────────────────────────────────

    [Fact]
    public void ResearchRequestedEvent_CreatesWithAllProperties()
    {
        var timestamp = DateTimeOffset.UtcNow;
        var evt = new ResearchRequestedEvent(
            Timestamp: timestamp,
            RequestingAgent: AgentRole.Coder,
            Query: "Polly retry patterns",
            Scope: ResearchScope.All
        );

        Assert.Equal(timestamp, evt.Timestamp);
        Assert.Equal(AgentRole.Coder, evt.RequestingAgent);
        Assert.Equal("Polly retry patterns", evt.Query);
        Assert.Equal(ResearchScope.All, evt.Scope);
    }

    [Fact]
    public void ResearchCompletedEvent_CreatesWithMetrics()
    {
        var timestamp = DateTimeOffset.UtcNow;
        var evt = new ResearchCompletedEvent(
            Timestamp: timestamp,
            RequestingAgent: AgentRole.Designer,
            SourcesFound: 5,
            Duration: TimeSpan.FromSeconds(3.2)
        );

        Assert.Equal(timestamp, evt.Timestamp);
        Assert.Equal(AgentRole.Designer, evt.RequestingAgent);
        Assert.Equal(5, evt.SourcesFound);
        Assert.Equal(TimeSpan.FromSeconds(3.2), evt.Duration);
    }

    [Fact]
    public void AgentCommunicationEvent_CreatesWithCommunicationType()
    {
        var timestamp = DateTimeOffset.UtcNow;
        var evt = new AgentCommunicationEvent(
            Timestamp: timestamp,
            FromAgent: AgentRole.Orchestrator,
            ToAgent: AgentRole.Researcher,
            CommunicationType: "research_request",
            Summary: "Requesting HttpClient patterns"
        );

        Assert.Equal(timestamp, evt.Timestamp);
        Assert.Equal(AgentRole.Orchestrator, evt.FromAgent);
        Assert.Equal(AgentRole.Researcher, evt.ToAgent);
        Assert.Equal("research_request", evt.CommunicationType);
        Assert.Equal("Requesting HttpClient patterns", evt.Summary);
    }

    [Fact]
    public void ResearchEvents_InheritFromOrchestrationEvent()
    {
        var researchRequested = new ResearchRequestedEvent(DateTimeOffset.UtcNow, AgentRole.Coder, "query", ResearchScope.All);
        var researchCompleted = new ResearchCompletedEvent(DateTimeOffset.UtcNow, AgentRole.Coder, 3, TimeSpan.FromSeconds(1));
        var agentComm = new AgentCommunicationEvent(DateTimeOffset.UtcNow, AgentRole.Orchestrator, AgentRole.Researcher, "research", "summary");

        Assert.IsAssignableFrom<OrchestrationEvent>(researchRequested);
        Assert.IsAssignableFrom<OrchestrationEvent>(researchCompleted);
        Assert.IsAssignableFrom<OrchestrationEvent>(agentComm);
    }

    [Fact]
    public void OrchestrationCompletedEvent_IncludesFlowVisualizationData()
    {
        var timestamp = DateTimeOffset.UtcNow;
        var result = new OrchestrationResult("Test summary", Array.Empty<TaskResult>(), "/workspace");
        var callGraph = new AgentCallGraph();
        callGraph.RecordCall(AgentRole.Orchestrator, AgentRole.Planner, "Plan", timestamp);

        var evt = new OrchestrationCompletedEvent(
            Timestamp: timestamp,
            FinalResult: result,
            AgentFlowDiagram: "```mermaid...```",
            AgentFlowJson: "{\"nodes\":[]}",
            CallGraph: callGraph,
            TotalAgentCalls: 5,
            LoopIterations: 2
        );

        Assert.NotNull(evt.AgentFlowDiagram);
        Assert.Contains("mermaid", evt.AgentFlowDiagram);
        Assert.NotNull(evt.AgentFlowJson);
        Assert.NotNull(evt.CallGraph);
        Assert.Equal(5, evt.TotalAgentCalls);
        Assert.Equal(2, evt.LoopIterations);
    }

    // ──────────────────────────────────────
    // TemplateAgentClient - Researcher Role
    // ──────────────────────────────────────

    [Fact]
    public async Task TemplateAgentClient_Researcher_ReturnsStructuredResponse()
    {
        var client = new TemplateAgentClient();

        var result = await client.RunAsync(
            AgentRole.Researcher,
            "research HttpClient retry patterns",
            "/workspace",
            CancellationToken.None
        );

        Assert.Contains("# Research Summary", result);
        Assert.Contains("## Key Findings", result);
        Assert.Contains("## Sources", result);
        Assert.Contains("## Recommendations", result);
    }

    [Fact]
    public async Task TemplateAgentClient_Researcher_HttpClientQuery_ReturnsPollyInfo()
    {
        var client = new TemplateAgentClient();

        var result = await client.RunAsync(
            AgentRole.Researcher,
            "How to implement retry policies with HttpClient?",
            "/workspace",
            CancellationToken.None
        );

        Assert.Contains("Polly", result);
        Assert.Contains("IHttpClientFactory", result);
        Assert.Contains("exponential backoff", result);
        Assert.Contains("Microsoft Learn", result);
    }

    [Fact]
    public async Task TemplateAgentClient_Researcher_BuildErrorQuery_ReturnsSolutions()
    {
        var client = new TemplateAgentClient();

        var result = await client.RunAsync(
            AgentRole.Researcher,
            "CS0246 error: The type 'MyCustomType' could not be found",
            "/workspace",
            CancellationToken.None
        );

        Assert.Contains("Missing Using Statements", result);
        Assert.Contains("CS0246", result);
        Assert.Contains("using", result);
        Assert.Contains("SDK Version", result);
    }

    [Fact]
    public async Task TemplateAgentClient_Researcher_CssQuery_ReturnsGridInfo()
    {
        var client = new TemplateAgentClient();

        var result = await client.RunAsync(
            AgentRole.Researcher,
            "Modern CSS Grid layout patterns",
            "/workspace",
            CancellationToken.None
        );

        Assert.Contains("CSS Grid", result);
        Assert.Contains("grid-template", result);
        Assert.Contains("responsive", result);
        Assert.Contains("MDN", result);
    }

    [Fact]
    public async Task TemplateAgentClient_Researcher_GenericQuery_ReturnsGenericResponse()
    {
        var client = new TemplateAgentClient();

        var result = await client.RunAsync(
            AgentRole.Researcher,
            "some random topic",
            "/workspace",
            CancellationToken.None
        );

        Assert.Contains("Research completed", result);
        Assert.Contains("## Key Findings", result);
        Assert.Contains("Microsoft Learn Documentation", result);
    }

    [Fact]
    public async Task TemplateAgentClient_Researcher_IncludesSourceTypes()
    {
        var client = new TemplateAgentClient();

        var result = await client.RunAsync(
            AgentRole.Researcher,
            "Polly retry patterns",
            "/workspace",
            CancellationToken.None
        );

        Assert.Contains("**Type**: docs", result);
        Assert.Contains("**Type**: library-docs", result);
        Assert.Contains("**Type**: web", result);
    }

    [Fact]
    public async Task TemplateAgentClient_Researcher_IncludesUrls()
    {
        var client = new TemplateAgentClient();

        var result = await client.RunAsync(
            AgentRole.Researcher,
            "retry patterns",
            "/workspace",
            CancellationToken.None
        );

        Assert.Contains("**URL**:", result);
        Assert.Contains("https://", result);
    }

    // ──────────────────────────────────────
    // Integration: Researcher in Configuration Store
    // ──────────────────────────────────────

    [Fact]
    public void AgentConfigurationStore_IncludesResearcher()
    {
        var store = new AgentConfigurationStore();

        var researcher = store.Get(AgentRole.Researcher);

        Assert.Equal(AgentRole.Researcher, researcher.Role);
        Assert.Equal("Researcher", researcher.Name);
        Assert.Equal("🔍", researcher.Icon);
        Assert.Equal("#6610f2", researcher.Color);
        Assert.NotEmpty(researcher.Instructions);
    }

    [Fact]
    public void AgentConfigurationStore_ResearcherHasCorrectInstructions()
    {
        var store = new AgentConfigurationStore();
        var researcher = store.Get(AgentRole.Researcher);

        Assert.Contains("research", researcher.Instructions, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AgentConfigurationStore_CanUpdateResearcherTools()
    {
        var store = new AgentConfigurationStore();
        var original = store.Get(AgentRole.Researcher);

        var updated = original with
        {
            Tools = new AgentToolConfiguration(
                WebSearchEnabled: false,
                MicrosoftLearnMcpEnabled: true,
                Context7McpEnabled: false
            )
        };

        store.Update(updated);
        var retrieved = store.Get(AgentRole.Researcher);

        Assert.False(retrieved.Tools!.WebSearchEnabled);
        Assert.True(retrieved.Tools.MicrosoftLearnMcpEnabled);
        Assert.False(retrieved.Tools.Context7McpEnabled);
    }
}
