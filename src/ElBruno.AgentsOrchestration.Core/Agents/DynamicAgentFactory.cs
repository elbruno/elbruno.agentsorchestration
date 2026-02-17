using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.AgentRepository.Services;

namespace ElBruno.AgentsOrchestration.Core.Agents;

/// <summary>
/// Extended agent factory that supports both static and dynamic agents.
/// </summary>
public sealed class DynamicAgentFactory
{
    private readonly AgentFactory _staticFactory;
    private readonly DynamicAgentManager? _dynamicManager;
    private readonly IAgentClient _agentClient;

    public DynamicAgentFactory(
        AgentFactory staticFactory,
        IAgentClient agentClient,
        DynamicAgentManager? dynamicManager = null)
    {
        _staticFactory = staticFactory;
        _agentClient = agentClient;
        _dynamicManager = dynamicManager;
    }

    /// <summary>
    /// Creates a session for a static agent role.
    /// </summary>
    public AgentSession CreateSession(AgentRole role) => _staticFactory.CreateSession(role);

    /// <summary>
    /// Creates a session for a dynamic agent by name.
    /// Returns null if the agent is not found.
    /// </summary>
    public AgentSession? CreateSessionByName(string agentName)
    {
        if (_dynamicManager == null)
        {
            return null;
        }

        var config = _dynamicManager.GetByName(agentName);
        if (config == null)
        {
            return null;
        }

        return new AgentSession(config, _agentClient);
    }

    /// <summary>
    /// Tries to create a session for an agent by name, falling back to static agents if not found in dynamic.
    /// </summary>
    public AgentSession? TryCreateSession(string agentName)
    {
        // Try dynamic agents first
        var session = CreateSessionByName(agentName);
        if (session != null)
        {
            return session;
        }

        // Try matching by static agent role name
        if (Enum.TryParse<AgentRole>(agentName, ignoreCase: true, out var role))
        {
            return CreateSession(role);
        }

        return null;
    }

    /// <summary>
    /// Gets all available agent configurations (static + dynamic).
    /// </summary>
    public IReadOnlyCollection<AgentConfiguration> GetAllAgents()
    {
        if (_dynamicManager == null)
        {
            // Return only static agents
            return Array.Empty<AgentConfiguration>();
        }

        return _dynamicManager.GetAllAgents();
    }

    /// <summary>
    /// Checks if an agent with the given name exists.
    /// </summary>
    public bool HasAgent(string agentName)
    {
        if (_dynamicManager?.HasAgent(agentName) == true)
        {
            return true;
        }

        return Enum.TryParse<AgentRole>(agentName, ignoreCase: true, out _);
    }
}
