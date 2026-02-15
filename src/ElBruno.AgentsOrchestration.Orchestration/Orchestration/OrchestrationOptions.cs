using ElBruno.AgentsOrchestration.Agents;

namespace ElBruno.AgentsOrchestration.Orchestration;

/// <summary>
/// Configuration options for the orchestration service.
/// </summary>
public sealed class OrchestrationOptions
{
    /// <summary>
    /// Gets or sets the root path for workspace directories.
    /// Each orchestration run creates a timestamped subdirectory under this path.
    /// Default: {TEMP}/orchestration-workspaces
    /// </summary>
    public string WorkspaceRoot { get; set; } = Path.Combine(Path.GetTempPath(), "orchestration-workspaces");

    /// <summary>
    /// Gets or sets the maximum number of fix attempts if the build fails.
    /// Valid range: 0-10. Default: 3
    /// </summary>
    public int MaxFixAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets a custom agent client implementation.
    /// If null, the default CopilotAgentClient is used.
    /// </summary>
    public IAgentClient? CustomClient { get; set; }

    /// <summary>
    /// Gets or sets custom instructions for agents.
    /// If null, instructions are loaded from the default sources (environment variable, embedded files, or built-in defaults).
    /// </summary>
    public IReadOnlyDictionary<AgentRole, string>? CustomInstructions { get; set; }
}
