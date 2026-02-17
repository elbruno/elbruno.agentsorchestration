using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.AgentRepository.Models;
using ElBruno.AgentsOrchestration.AgentRepository.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring dynamic agent support in an IServiceCollection.
/// </summary>
public static class DynamicAgentServiceCollectionExtensions
{
    /// <summary>
    /// Adds dynamic agent support to the orchestration service.
    /// This must be called after AddOrchestration().
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional action to configure AgentRepositoryOptions.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDynamicAgents(
        this IServiceCollection services,
        Action<AgentRepositoryOptions>? configure = null)
    {
        // Register AgentRepositoryOptions
        if (configure is not null)
        {
            services.Configure(configure);
        }

        // Register AwesomeCopilotAgentLoader (singleton for caching)
        services.TryAddSingleton<AwesomeCopilotAgentLoader>(sp =>
        {
            var options = sp.GetService<Microsoft.Extensions.Options.IOptions<AgentRepositoryOptions>>()?.Value;
            return new AwesomeCopilotAgentLoader(options);
        });

        // Register DynamicAgentManager (singleton)
        services.TryAddSingleton<DynamicAgentManager>(sp =>
        {
            var staticStore = sp.GetRequiredService<AgentConfigurationStore>();
            var loader = sp.GetRequiredService<AwesomeCopilotAgentLoader>();
            return new DynamicAgentManager(staticStore, loader);
        });

        return services;
    }
}
