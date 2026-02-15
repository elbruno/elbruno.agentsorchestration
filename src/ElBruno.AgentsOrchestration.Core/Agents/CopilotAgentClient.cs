using System.Text;
using GitHub.Copilot.SDK;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.GitHub.Copilot;

namespace ElBruno.AgentsOrchestration.Agents;

/// <summary>
/// IAgentClient implementation backed by GitHub Copilot via the Copilot SDK.
/// Uses the Microsoft Agents AI bridge (GitHubCopilotAgent) to communicate
/// with the Copilot instance running in VS Code.
/// </summary>
public sealed class CopilotAgentClient : IAgentClient
{
    private readonly CopilotClient _copilotClient;
    private readonly AgentConfigurationStore _store;

    public CopilotAgentClient(CopilotClient copilotClient, AgentConfigurationStore store)
    {
        _copilotClient = copilotClient;
        _store = store;
    }

    public async Task<string> RunAsync(AgentRole role, string prompt, string workspacePath, CancellationToken cancellationToken)
    {
        var config = _store.Get(role);
        var agent = _copilotClient.AsAIAgent(tools: null);

        var fullPrompt = $"""
            {config.Instructions}

            Workspace: {workspacePath}

            {prompt}
            """;

        var result = new StringBuilder();
        await foreach (var update in agent.RunStreamingAsync(fullPrompt, cancellationToken: cancellationToken))
        {
            if (update.Text is not null)
                result.Append(update.Text);
        }

        return result.ToString();
    }
}
