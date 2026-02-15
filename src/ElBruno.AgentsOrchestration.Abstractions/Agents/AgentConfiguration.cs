namespace ElBruno.AgentsOrchestration.Agents;

public sealed record AgentConfiguration(
    AgentRole Role,
    string Name,
    string Model,
    string Instructions,
    string Color,
    string Icon,
    AgentToolConfiguration? Tools = null
);

/// <summary>
/// Configuration for Copilot tools available to an agent.
/// </summary>
public sealed record AgentToolConfiguration(
    bool WebSearchEnabled = false,
    bool MicrosoftLearnMcpEnabled = false,
    bool Context7McpEnabled = false,
    IReadOnlyList<string>? CustomMcpServers = null
);
