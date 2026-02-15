using ElBruno.AgentsOrchestration.Agents;

namespace ElBruno.AgentsOrchestration.Orchestration;

public sealed record OrchestrationRequest(string Prompt);

public sealed record ExecutionTask(string Description, AgentRole AssignedRole, string FileScope);

public sealed record ExecutionPhase(string Name, IReadOnlyCollection<ExecutionTask> Tasks);

public sealed record ExecutionPlan(IReadOnlyCollection<ExecutionPhase> Phases);

public sealed record TaskResult(AgentRole Role, string Description, string Output);

public sealed record OrchestrationResult(string Summary, IReadOnlyCollection<TaskResult> TaskResults, string WorkspacePath);

/// <summary>
/// A single turn in a multi-turn conversation.
/// </summary>
public sealed record ConversationTurn(
    string UserPrompt,
    OrchestrationResult Result,
    DateTimeOffset Timestamp
);

/// <summary>
/// Tracks a multi-turn conversation with workspace context.
/// </summary>
public sealed record ConversationSession(
    string SessionId,
    string WorkspacePath,
    IReadOnlyList<ConversationTurn> History,
    DateTimeOffset CreatedAt
)
{
    /// <summary>
    /// Builds a context prompt that includes conversation history for multi-turn orchestration.
    /// </summary>
    public string BuildContextPrompt(string latestUserPrompt)
    {
        if (History.Count == 0)
            return latestUserPrompt;

        var historyLines = new List<string> { "Conversation so far:" };
        foreach (var turn in History)
        {
            historyLines.Add($"[User]: {turn.UserPrompt}");
            historyLines.Add($"[Assistant]: {turn.Result.Summary}");
        }
        historyLines.Add($"\n[User]: {latestUserPrompt}");

        return string.Join("\n", historyLines);
    }

    /// <summary>
    /// Adds a new turn to the conversation history.
    /// </summary>
    public ConversationSession AddTurn(string userPrompt, OrchestrationResult result) =>
        this with { History = [.. History, new ConversationTurn(userPrompt, result, DateTimeOffset.UtcNow)] };
}

// ============================================================================
// Research Agent Models
// ============================================================================

/// <summary>
/// Scope of research to perform.
/// </summary>
public enum ResearchScope
{
    WebSearch,
    Documentation,
    CodeExamples,
    BestPractices,
    All
}

/// <summary>
/// Request for research to be performed by the Researcher agent.
/// </summary>
public sealed record ResearchRequest(
    AgentRole RequestingAgent,
    string Query,
    string Context,
    ResearchScope Scope,
    int MaxResults = 5
);

/// <summary>
/// Response from the Researcher agent with sources and summary.
/// </summary>
public sealed record ResearchResponse(
    ResearchRequest Request,
    string Summary,
    IReadOnlyList<ResearchSource> Sources,
    DateTimeOffset CompletedAt
);

/// <summary>
/// A single research source (web page, documentation, code example, etc.).
/// </summary>
public sealed record ResearchSource(
    string Title,
    string Url,
    string Excerpt,
    string SourceType // "web", "docs", "github", "library-docs", etc.
);

// ============================================================================
// Agent Call Flow Tracking
// ============================================================================

/// <summary>
/// Represents a single agent-to-agent communication.
/// </summary>
public sealed record AgentCall(
    AgentRole FromAgent,
    AgentRole ToAgent,
    string Purpose,
    DateTimeOffset Timestamp,
    TimeSpan? Duration = null,
    int? AttemptNumber = null
);

/// <summary>
/// Tracks all agent interactions during an orchestration session.
/// </summary>
public sealed class AgentCallGraph
{
    private readonly List<AgentCall> _calls = new();

    public IReadOnlyList<AgentCall> Calls => _calls.AsReadOnly();

    public void RecordCall(AgentRole from, AgentRole to, string purpose, DateTimeOffset timestamp, TimeSpan? duration = null, int? attemptNumber = null)
    {
        _calls.Add(new AgentCall(from, to, purpose, timestamp, duration, attemptNumber));
    }
}

// ============================================================================
// Orchestration Configuration
// ============================================================================

/// <summary>
/// Configuration options for orchestration behavior including tracing.
/// </summary>
public sealed record OrchestrationConfiguration(
    bool TracingEnabled = true,
    bool GenerateMermaidDiagram = true,
    bool GenerateJsonFlow = true,
    int MaxTrackedCalls = 1000,
    int MaxRetryAttempts = 3,
    int ResearchTriggerThreshold = 3
);
