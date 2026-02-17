namespace ElBruno.AgentsOrchestration.AgentRepository.Models;

/// <summary>
/// Represents an agent definition loaded from an external source (e.g., Awesome Copilot Repository).
/// </summary>
public sealed record DynamicAgentDefinition(
    string Name,
    string Description,
    string Instructions,
    string? Model = null,
    string? Version = null,
    IReadOnlyList<string>? Tools = null,
    IReadOnlyDictionary<string, string>? Metadata = null
);

/// <summary>
/// Represents metadata from the YAML front matter of an agent definition file.
/// </summary>
public sealed record AgentFrontMatter(
    string Name,
    string Description,
    string? Model = null,
    string? Version = null,
    IReadOnlyList<string>? Tools = null
);
