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

    /// <summary>
    /// Estimates the token count for the entire conversation history.
    /// Uses a rough approximation: 1 token ≈ 4 characters.
    /// </summary>
    public int EstimateTokenCount()
    {
        var totalChars = 0;
        foreach (var turn in History)
        {
            totalChars += turn.UserPrompt.Length;
            totalChars += turn.Result.Summary.Length;

            // Include task results in estimation
            foreach (var taskResult in turn.Result.TaskResults)
            {
                totalChars += taskResult.Description.Length;
                totalChars += taskResult.Output.Length;
            }
        }

        // Rough estimate: 1 token per 4 characters
        return totalChars / 4;
    }

    /// <summary>
    /// Trims conversation history to fit within a maximum token budget.
    /// Always preserves the first turn (context anchor). Removes oldest turns first.
    /// </summary>
    /// <param name="maxTokens">Maximum allowed token count</param>
    /// <returns>New session with trimmed history, or original if already within budget</returns>
    public ConversationSession TrimHistory(int maxTokens)
    {
        if (maxTokens <= 0)
            throw new ArgumentException("Max tokens must be positive", nameof(maxTokens));

        var currentTokens = EstimateTokenCount();
        if (currentTokens <= maxTokens)
            return this;

        if (History.Count == 0)
            return this;

        // Always preserve first turn (context anchor)
        var newHistory = new List<ConversationTurn> { History[0] };
        var tokensUsed = EstimateTokensForTurn(History[0]);

        // Add turns from newest to oldest until we exceed budget
        for (int i = History.Count - 1; i > 0; i--)
        {
            var turn = History[i];
            var turnTokens = EstimateTokensForTurn(turn);

            if (tokensUsed + turnTokens <= maxTokens)
            {
                newHistory.Insert(1, turn); // Insert after first turn
                tokensUsed += turnTokens;
            }
            else
            {
                break; // Would exceed budget
            }
        }

        return this with { History = newHistory.AsReadOnly() };
    }

    /// <summary>
    /// Keeps only the last N turns in the conversation history.
    /// Always preserves the first turn if N > 0.
    /// </summary>
    /// <param name="n">Number of turns to keep</param>
    /// <returns>New session with trimmed history</returns>
    public ConversationSession TrimToLastNTurns(int n)
    {
        if (n <= 0)
            throw new ArgumentException("Must keep at least 1 turn", nameof(n));

        if (History.Count <= n)
            return this;

        if (History.Count == 0)
            return this;

        // Always preserve first turn
        var newHistory = new List<ConversationTurn> { History[0] };

        // Add last (n-1) turns
        var startIndex = Math.Max(1, History.Count - (n - 1));
        for (int i = startIndex; i < History.Count; i++)
        {
            if (i != 0) // Don't duplicate first turn
                newHistory.Add(History[i]);
        }

        return this with { History = newHistory.AsReadOnly() };
    }

    private int EstimateTokensForTurn(ConversationTurn turn)
    {
        var chars = turn.UserPrompt.Length + turn.Result.Summary.Length;
        foreach (var taskResult in turn.Result.TaskResults)
        {
            chars += taskResult.Description.Length + taskResult.Output.Length;
        }
        return chars / 4;
    }
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
