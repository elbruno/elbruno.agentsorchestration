namespace ElBruno.AgentsOrchestration.AgentRepository.Models;

/// <summary>
/// Configuration options for the agent repository.
/// </summary>
public sealed record AgentRepositoryOptions
{
    /// <summary>
    /// Base URL for the Awesome Copilot Repository.
    /// </summary>
    public string RepositoryUrl { get; init; } = "https://raw.githubusercontent.com/github/awesome-copilot/main/agents/";

    /// <summary>
    /// Cache directory for downloaded agent definitions.
    /// </summary>
    public string? CacheDirectory { get; init; }

    /// <summary>
    /// Cache expiration time in hours.
    /// </summary>
    public int CacheExpirationHours { get; init; } = 24;

    /// <summary>
    /// HTTP timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 30;
}
