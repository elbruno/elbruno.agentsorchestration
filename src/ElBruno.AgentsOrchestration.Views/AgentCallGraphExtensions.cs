using System.Text;
using System.Text.Json;
using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.Orchestration;

namespace ElBruno.AgentsOrchestration.Views;

public static class AgentCallGraphExtensions
{
    public static string ToMermaidDiagram(this AgentCallGraph graph)
    {
        var sb = new StringBuilder();
        sb.AppendLine("```mermaid");
        sb.AppendLine("sequenceDiagram");
        sb.AppendLine("    participant O as Orchestrator");
        sb.AppendLine("    participant P as Planner");
        sb.AppendLine("    participant C as Coder");
        sb.AppendLine("    participant D as Designer");
        sb.AppendLine("    participant R as Researcher");
        sb.AppendLine("    participant F as Fixer");
        sb.AppendLine("    participant B as BuildReviewer");
        sb.AppendLine();

        foreach (var call in graph.Calls)
        {
            var fromAbbrev = GetAbbreviation(call.FromAgent);
            var toAbbrev = GetAbbreviation(call.ToAgent);
            var purpose = call.Purpose.Replace("\"", "'");

            if (call.AttemptNumber.HasValue && call.AttemptNumber.Value > 1)
            {
                sb.AppendLine($"    Note over {toAbbrev}: Attempt {call.AttemptNumber} - {purpose}");
            }

            sb.AppendLine($"    {fromAbbrev}->>{toAbbrev}: {purpose}");
        }

        sb.AppendLine("```");
        return sb.ToString();
    }

    public static string ToJsonFlow(this AgentCallGraph graph)
    {
        var nodes = graph.Calls
            .SelectMany(c => new[] { c.FromAgent, c.ToAgent })
            .Distinct()
            .Select(role => new
            {
                id = role.ToString().ToLowerInvariant(),
                label = role.ToString(),
                callCount = graph.Calls.Count(c => c.FromAgent == role || c.ToAgent == role)
            });

        var edges = graph.Calls
            .GroupBy(c => (c.FromAgent, c.ToAgent, c.Purpose))
            .Select(g => new
            {
                from = g.Key.FromAgent.ToString().ToLowerInvariant(),
                to = g.Key.ToAgent.ToString().ToLowerInvariant(),
                label = g.Key.Purpose,
                count = g.Count()
            });

        var loops = graph.Calls
            .Where(c => c.AttemptNumber.HasValue && c.AttemptNumber > 1)
            .GroupBy(c => c.ToAgent)
            .Select(g => new
            {
                agent = g.Key.ToString().ToLowerInvariant(),
                iterations = g.Max(c => c.AttemptNumber ?? 1),
                reason = string.Join(", ", g.Select(c => c.Purpose).Distinct())
            });

        return JsonSerializer.Serialize(new { nodes, edges, loops }, new JsonSerializerOptions { WriteIndented = true });
    }

    public static string ToAsciiFlow(this AgentCallGraph graph)
    {
        var sb = new StringBuilder();
        sb.AppendLine("┌──────────────┐");
        sb.AppendLine("│ ORCHESTRATOR │");
        sb.AppendLine("└──────┬───────┘");
        sb.AppendLine("       │");

        foreach (var call in graph.Calls)
        {
            var role = call.ToAgent.ToString().ToUpper();
            // Truncate long purpose strings for cleaner display
            var task = call.Purpose.Length > 40 ? call.Purpose[..37] + "..." : call.Purpose;

            sb.AppendLine($"       │  [{task}]");
            sb.AppendLine($"       ├──► {role}");

            if (call.ToAgent == AgentRole.Fixer)
            {
                sb.AppendLine("       │    (Self-Healing)");
            }
        }

        sb.AppendLine("       │");
        sb.AppendLine("◄──────┘ (End)");

        return sb.ToString();
    }

    private static string GetAbbreviation(AgentRole role) => role switch
    {
        AgentRole.Orchestrator => "O",
        AgentRole.Planner => "P",
        AgentRole.Coder => "C",
        AgentRole.Designer => "D",
        AgentRole.Researcher => "R",
        AgentRole.Fixer => "F",
        AgentRole.BuildReviewer => "B",
        AgentRole.SecurityExpert => "S",
        AgentRole.TestingExpert => "T",
        AgentRole.DocumentationExpert => "Doc",
        AgentRole.SoftwareArchitect => "A",
        _ => role.ToString()[0].ToString()
    };
}