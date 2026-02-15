using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.Core.Orchestration;
using ElBruno.AgentsOrchestration.Orchestration;
using ElBruno.AgentsOrchestration.Workspace;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring OrchestrationService in an IServiceCollection.
/// </summary>
public static class OrchestrationServiceCollectionExtensions
{
    /// <summary>
    /// Adds the OrchestrationService and its dependencies to the service collection.
    /// Uses TemplateAgentClient as the default IAgentClient implementation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional action to configure OrchestrationOptions.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOrchestration(
        this IServiceCollection services,
        Action<OrchestrationOptions>? configure = null)
    {
        // Register options
        if (configure is not null)
        {
            services.Configure(configure);
        }
        else
        {
            services.Configure<OrchestrationOptions>(_ => { });
        }

        // Register agent instructions (singleton shared across all agents)
        services.TryAddSingleton<IReadOnlyDictionary<AgentRole, string>>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<OrchestrationOptions>>().Value;
            return options.CustomInstructions ?? InstructionLoader.LoadInstructions();
        });

        // Register configuration store (singleton)
        services.TryAddSingleton<AgentConfigurationStore>(sp =>
        {
            var instructions = sp.GetRequiredService<IReadOnlyDictionary<AgentRole, string>>();
            return new AgentConfigurationStore(instructions);
        });

        // Register default agent client (singleton unless overridden)
        services.TryAddSingleton<IAgentClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<OrchestrationOptions>>().Value;
            return options.CustomClient ?? new TemplateAgentClient();
        });

        // Register agent factory (singleton)
        services.TryAddSingleton<AgentFactory>();

        // Register agent output store (singleton)
        services.TryAddSingleton<ElBruno.AgentsOrchestration.Orchestration.AgentOutputStore>();

        // Register workspace manager (scoped - new workspace per request)
        services.TryAddScoped<IWorkspace>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<OrchestrationOptions>>().Value;
            return new WorkspaceManager(options.WorkspaceRoot);
        });

        // Register orchestration service (scoped - new instance per HTTP request or scope)
        services.TryAddScoped<OrchestrationService>(sp =>
        {
            var factory = sp.GetRequiredService<AgentFactory>();
            var workspace = sp.GetRequiredService<IWorkspace>();
            var outputStore = sp.GetRequiredService<ElBruno.AgentsOrchestration.Orchestration.AgentOutputStore>();
            var options = sp.GetRequiredService<IOptions<OrchestrationOptions>>().Value;
            return new OrchestrationService(factory, workspace, options.MaxFixAttempts, outputStore);
        });

        return services;
    }

    /// <summary>
    /// Adds the OrchestrationService with a custom IAgentClient implementation.
    /// </summary>
    /// <typeparam name="TClient">The custom agent client type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional action to configure OrchestrationOptions.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOrchestration<TClient>(
        this IServiceCollection services,
        Action<OrchestrationOptions>? configure = null)
        where TClient : class, IAgentClient
    {
        // First add the base orchestration services
        services.AddOrchestration(configure);

        // Then replace the IAgentClient registration with the custom type
        services.Replace(ServiceDescriptor.Singleton<IAgentClient, TClient>());

        return services;
    }

    /// <summary>
    /// Adds the OrchestrationService with CopilotAgentClient (requires GitHub Copilot SDK dependencies).
    /// You must manually register CopilotClient in the service collection before calling this method.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional action to configure OrchestrationOptions.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOrchestrationWithCopilot(
        this IServiceCollection services,
        Action<OrchestrationOptions>? configure = null)
    {
        // Note: CopilotClient must be registered separately by the caller
        // because it requires specific configuration (e.g., connection to VS Code)
        return services.AddOrchestration<CopilotAgentClient>(configure);
    }
}
