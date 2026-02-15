using System.Collections.Concurrent;

namespace ElBruno.AgentsOrchestration.Agents;

public sealed class AgentConfigurationStore
{
    private readonly ConcurrentDictionary<AgentRole, AgentConfiguration> _configs;
    private readonly IReadOnlyDictionary<AgentRole, string> _defaultInstructions;

    public AgentConfigurationStore(IReadOnlyDictionary<AgentRole, string>? defaultInstructions = null)
    {
        _defaultInstructions = defaultInstructions ?? CreateDefaultInstructions();
        _configs = new ConcurrentDictionary<AgentRole, AgentConfiguration>(
            CreateDefaultConfigurations(_defaultInstructions).ToDictionary(k => k.Role, v => v));
    }

    public IReadOnlyCollection<AgentConfiguration> GetAll() => _configs.Values.OrderBy(c => c.Role).ToArray();

    public AgentConfiguration Get(AgentRole role) => _configs[role];

    public void Update(AgentConfiguration configuration) => _configs[configuration.Role] = configuration;

    public void ResetInstructions(AgentRole role)
    {
        var existing = Get(role);
        Update(existing with { Instructions = _defaultInstructions[role] });
    }

    private static IReadOnlyDictionary<AgentRole, string> CreateDefaultInstructions() =>
        new Dictionary<AgentRole, string>
        {
            [AgentRole.Orchestrator] = "Coordinate planner/coder/designer and report progress.",
            [AgentRole.Planner] = "Create implementation plans that describe WHAT should be built.",
            [AgentRole.Coder] = "Implement features with clear, maintainable code.",
            [AgentRole.Designer] = "Improve UX/accessibility and styling details.",
            [AgentRole.Researcher] = "Perform research using web search and documentation tools to help other agents.",
            [AgentRole.Fixer] = "Analyze build errors and produce corrected source files.",
            [AgentRole.BuildReviewer] = "Review successful builds and provide quality feedback.",
            [AgentRole.SecurityExpert] = "Identify security vulnerabilities and enforce secure coding practices.",
            [AgentRole.TestingExpert] = "Generate comprehensive test suites and ensure test coverage.",
            [AgentRole.DocumentationExpert] = "Create comprehensive documentation including README, API docs, and diagrams.",
            [AgentRole.SoftwareArchitect] = "Validate architectural decisions and enforce design patterns."
        };

    private static IEnumerable<AgentConfiguration> CreateDefaultConfigurations(IReadOnlyDictionary<AgentRole, string> instructions)
    {
        yield return new AgentConfiguration(AgentRole.Orchestrator, "Orchestrator", "gpt-5.3-codex", instructions[AgentRole.Orchestrator], "#6c757d", "🧭");
        yield return new AgentConfiguration(AgentRole.Planner, "Planner", "gpt-5.3-codex", instructions[AgentRole.Planner], "#0d6efd", "🗺️");
        yield return new AgentConfiguration(AgentRole.Coder, "Coder", "gpt-5.3-codex", instructions[AgentRole.Coder], "#198754", "💻");
        yield return new AgentConfiguration(AgentRole.Designer, "Designer", "gpt-5.3-codex", instructions[AgentRole.Designer], "#d63384", "🎨");
        yield return new AgentConfiguration(
            AgentRole.Researcher,
            "Researcher",
            "gpt-5.3-codex",
            instructions[AgentRole.Researcher],
            "#6610f2",
            "🔍",
            new AgentToolConfiguration(
                WebSearchEnabled: true,
                MicrosoftLearnMcpEnabled: true,
                Context7McpEnabled: true
            )
        );
        yield return new AgentConfiguration(AgentRole.Fixer, "Fixer", "gpt-5.3-codex", instructions[AgentRole.Fixer], "#fd7e14", "🔧");
        yield return new AgentConfiguration(AgentRole.BuildReviewer, "BuildReviewer", "gpt-5.3-codex", instructions[AgentRole.BuildReviewer], "#20c997", "📊");
        yield return new AgentConfiguration(AgentRole.SecurityExpert, "SecurityExpert", "gpt-5.3-codex", instructions[AgentRole.SecurityExpert], "#dc3545", "🔒");
        yield return new AgentConfiguration(AgentRole.TestingExpert, "TestingExpert", "gpt-5.3-codex", instructions[AgentRole.TestingExpert], "#28a745", "🧪");
        yield return new AgentConfiguration(AgentRole.DocumentationExpert, "DocumentationExpert", "gpt-5.3-codex", instructions[AgentRole.DocumentationExpert], "#17a2b8", "📚");
        yield return new AgentConfiguration(AgentRole.SoftwareArchitect, "SoftwareArchitect", "gpt-5.3-codex", instructions[AgentRole.SoftwareArchitect], "#ffc107", "🏗️");
    }
}
