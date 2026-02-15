using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.Orchestration;
using ElBruno.AgentsOrchestration.Workspace;

namespace ElBruno.AgentsOrchestration.Core.Orchestration;

/// <summary>
/// Provides simplified creation of OrchestrationService instances with smart defaults.
/// </summary>
public static class OrchestrationServiceFactory
{
    /// <summary>
    /// Creates an OrchestrationService with default configuration.
    /// Uses TemplateAgentClient and creates a temporary workspace directory.
    /// </summary>
    public static OrchestrationService Create()
    {
        return Create((string?)null);
    }

    /// <summary>
    /// Creates an OrchestrationService with a custom workspace root path.
    /// </summary>
    /// <param name="workspaceRoot">Root directory for workspaces. If null, uses default temp directory.</param>
    /// <param name="maxFixAttempts">Maximum number of fix attempts on build failure (0-10).</param>
    public static OrchestrationService Create(string? workspaceRoot, int maxFixAttempts = 3)
    {
        workspaceRoot ??= Path.Combine(Path.GetTempPath(), "orchestration-workspaces");

        var instructions = InstructionLoader.LoadInstructions();
        var store = new AgentConfigurationStore(instructions);
        var client = new TemplateAgentClient();  // Default to template client for simplicity
        var factory = new AgentFactory(store, client);
        var workspace = new WorkspaceManager(workspaceRoot);

        return new OrchestrationService(factory, workspace, maxFixAttempts);
    }

    /// <summary>
    /// Creates an OrchestrationService with custom configuration options.
    /// </summary>
    /// <param name="configure">Action to configure OrchestrationOptions.</param>
    public static OrchestrationService Create(Action<OrchestrationOptions> configure)
    {
        var options = new OrchestrationOptions();
        configure(options);

        var instructions = options.CustomInstructions ?? InstructionLoader.LoadInstructions();
        var store = new AgentConfigurationStore(instructions);
        var client = options.CustomClient ?? new TemplateAgentClient();  // Default to template client
        var factory = new AgentFactory(store, client);
        var workspace = new WorkspaceManager(options.WorkspaceRoot);

        return new OrchestrationService(factory, workspace, options.MaxFixAttempts);
    }
}
