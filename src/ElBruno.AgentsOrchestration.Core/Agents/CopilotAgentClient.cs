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

        var roleContract = role switch
        {
            AgentRole.Planner =>
                """
                OUTPUT CONTRACT (STRICT):
                Return ONLY markdown (no prose before or after) in this exact structure:

                # Implementation Plan

                ## Phase 1: <name>
                - Task: <description> | Agent: <AgentRole> | File: <relative path>

                ## Phase 2: <name>
                - Task: <description> | Agent: <AgentRole> | File: <relative path>

                Rules:
                - Every task line MUST start with "- Task:" and use the exact pipe-separated format.
                - Agent must be one of: Orchestrator, Planner, Coder, Designer, Fixer, BuildReviewer, Researcher, SecurityExpert, TestingExpert, DocumentationExpert, SoftwareArchitect.
                - Use workspace-relative file paths.
                - Include a final Validation phase with:
                  - Task: Build and validate generated project | Agent: Orchestrator | File: build-output.log
                - Do NOT use code fences.
                """,
            AgentRole.Coder or AgentRole.Designer or AgentRole.Fixer =>
                """
                OUTPUT CONTRACT (STRICT):
                Return ONLY the final file content for the requested target file.
                - No explanations.
                - No markdown.
                - No surrounding text.
                - No code fences.
                """,
            _ => string.Empty
        };

        var fullPrompt = $"""
            {config.Instructions}

            {roleContract}

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
