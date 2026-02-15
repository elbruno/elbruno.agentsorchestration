namespace ElBruno.AgentsOrchestration.Agents;

public interface IAgentClient
{
    Task<string> RunAsync(AgentRole role, string prompt, string workspacePath, CancellationToken cancellationToken);
}
