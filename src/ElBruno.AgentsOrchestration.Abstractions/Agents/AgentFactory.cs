namespace ElBruno.AgentsOrchestration.Agents;

public sealed class AgentFactory
{
    private readonly AgentConfigurationStore _store;
    private readonly IAgentClient _agentClient;

    public AgentFactory(AgentConfigurationStore store, IAgentClient agentClient)
    {
        _store = store;
        _agentClient = agentClient;
    }

    public AgentSession CreateSession(AgentRole role) => new(_store.Get(role), _agentClient);
}

public sealed class AgentSession
{
    private readonly IAgentClient _agentClient;

    public AgentSession(AgentConfiguration configuration, IAgentClient agentClient)
    {
        Configuration = configuration;
        _agentClient = agentClient;
    }

    public AgentConfiguration Configuration { get; }

    public Task<string> RunAsync(string prompt, string workspacePath, CancellationToken cancellationToken) =>
        _agentClient.RunAsync(Configuration.Role, prompt, workspacePath, cancellationToken);
}
