using System.Collections.Concurrent;
using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.AgentRepository.Models;

namespace ElBruno.AgentsOrchestration.AgentRepository.Services;

/// <summary>
/// Manages both static and dynamic agent configurations.
/// Extends the standard AgentConfigurationStore with dynamic agent support.
/// </summary>
public sealed class DynamicAgentManager
{
    private readonly AgentConfigurationStore _staticStore;
    private readonly ConcurrentDictionary<string, AgentConfiguration> _dynamicAgents = new(StringComparer.OrdinalIgnoreCase);
    private readonly AwesomeCopilotAgentLoader _loader;

    public DynamicAgentManager(
        AgentConfigurationStore staticStore,
        AwesomeCopilotAgentLoader loader)
    {
        _staticStore = staticStore;
        _loader = loader;
    }

    /// <summary>
    /// Gets all available agents (static + dynamic).
    /// </summary>
    public IReadOnlyCollection<AgentConfiguration> GetAllAgents()
    {
        var staticAgents = _staticStore.GetAll();
        var dynamicAgents = _dynamicAgents.Values;
        return staticAgents.Concat(dynamicAgents).OrderBy(a => a.Name).ToArray();
    }

    /// <summary>
    /// Gets an agent configuration by role (static agents only).
    /// </summary>
    public AgentConfiguration GetByRole(AgentRole role) => _staticStore.Get(role);

    /// <summary>
    /// Gets an agent configuration by name (dynamic or static).
    /// </summary>
    public AgentConfiguration? GetByName(string name)
    {
        // Try dynamic agents first
        if (_dynamicAgents.TryGetValue(name, out var dynamicConfig))
        {
            return dynamicConfig;
        }

        // Try matching static agents by name
        return _staticStore.GetAll().FirstOrDefault(a => 
            a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if an agent with the given name exists.
    /// </summary>
    public bool HasAgent(string name) => GetByName(name) != null;

    /// <summary>
    /// Loads and registers a dynamic agent from the Awesome Copilot Repository.
    /// </summary>
    public async Task<AgentConfiguration> LoadAndRegisterAgentAsync(
        string agentId, 
        CancellationToken cancellationToken = default)
    {
        var definition = await _loader.LoadAgentAsync(agentId, cancellationToken);
        return RegisterDynamicAgent(definition);
    }

    /// <summary>
    /// Loads and registers a dynamic agent from a URL.
    /// </summary>
    public async Task<AgentConfiguration> LoadAndRegisterAgentFromUrlAsync(
        string url,
        string? agentId = null,
        CancellationToken cancellationToken = default)
    {
        var definition = await _loader.LoadAgentFromUrlAsync(url, agentId, cancellationToken);
        return RegisterDynamicAgent(definition);
    }

    /// <summary>
    /// Loads and registers a dynamic agent from a local file.
    /// </summary>
    public async Task<AgentConfiguration> LoadAndRegisterAgentFromFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var definition = await _loader.LoadAgentFromFileAsync(filePath, cancellationToken);
        return RegisterDynamicAgent(definition);
    }

    /// <summary>
    /// Registers a dynamic agent from a definition.
    /// </summary>
    public AgentConfiguration RegisterDynamicAgent(DynamicAgentDefinition definition)
    {
        // Create a synthetic AgentRole for dynamic agents
        // We use the last enum value + hash to avoid conflicts
        var syntheticRole = (AgentRole)int.MaxValue - Math.Abs(definition.Name.GetHashCode() % 10000);

        var config = new AgentConfiguration(
            Role: syntheticRole,
            Name: definition.Name,
            Model: definition.Model ?? "gpt-5.3-codex",
            Instructions: definition.Instructions,
            Color: GetColorForDynamicAgent(definition),
            Icon: GetIconForDynamicAgent(definition),
            Tools: CreateToolsConfiguration(definition.Tools)
        );

        _dynamicAgents[definition.Name] = config;
        return config;
    }

    /// <summary>
    /// Removes a dynamic agent by name.
    /// </summary>
    public bool RemoveDynamicAgent(string name)
    {
        return _dynamicAgents.TryRemove(name, out _);
    }

    /// <summary>
    /// Gets all dynamic agent names.
    /// </summary>
    public IReadOnlyList<string> GetDynamicAgentNames()
    {
        return _dynamicAgents.Keys.OrderBy(k => k).ToArray();
    }

    private static string GetColorForDynamicAgent(DynamicAgentDefinition definition)
    {
        // Assign colors based on name hash for consistency
        var colors = new[] { "#6c757d", "#0d6efd", "#198754", "#d63384", "#6610f2", "#fd7e14", "#20c997", "#dc3545", "#17a2b8", "#ffc107" };
        var index = Math.Abs(definition.Name.GetHashCode()) % colors.Length;
        return colors[index];
    }

    private static string GetIconForDynamicAgent(DynamicAgentDefinition definition)
    {
        // Map common agent types to icons
        var nameLower = definition.Name.ToLowerInvariant();
        
        if (nameLower.Contains("security") || nameLower.Contains("sec"))
            return "🔒";
        if (nameLower.Contains("test"))
            return "🧪";
        if (nameLower.Contains("doc"))
            return "📚";
        if (nameLower.Contains("ui") || nameLower.Contains("design"))
            return "🎨";
        if (nameLower.Contains("data") || nameLower.Contains("database"))
            return "💾";
        if (nameLower.Contains("api"))
            return "🔌";
        if (nameLower.Contains("expert") || nameLower.Contains("specialist"))
            return "👨‍💼";
        if (nameLower.Contains("review"))
            return "👁️";
        
        return "🤖"; // Default icon for dynamic agents
    }

    private static AgentToolConfiguration? CreateToolsConfiguration(IReadOnlyList<string>? tools)
    {
        if (tools == null || tools.Count == 0)
        {
            return null;
        }

        var webSearchEnabled = tools.Any(t => 
            t.Contains("search", StringComparison.OrdinalIgnoreCase) ||
            t.Contains("web", StringComparison.OrdinalIgnoreCase));

        return new AgentToolConfiguration(
            WebSearchEnabled: webSearchEnabled,
            MicrosoftLearnMcpEnabled: false,
            Context7McpEnabled: false
        );
    }
}
