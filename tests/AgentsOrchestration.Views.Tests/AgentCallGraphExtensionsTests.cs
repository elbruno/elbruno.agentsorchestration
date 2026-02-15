using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.Orchestration;
using ElBruno.AgentsOrchestration.Views;
using Xunit;

namespace AgentsOrchestration.Views.Tests;

public class AgentCallGraphExtensionsTests
{
    [Fact]
    public void ToMermaidDiagram_GeneratesCorrectSyntax()
    {
        // Arrange
        var graph = new AgentCallGraph();
        graph.RecordCall(AgentRole.Orchestrator, AgentRole.Planner, "Plan", DateTimeOffset.UtcNow);
        graph.RecordCall(AgentRole.Orchestrator, AgentRole.Coder, "Code", DateTimeOffset.UtcNow);

        // Act
        var diagram = graph.ToMermaidDiagram();

        // Assert
        Assert.Contains("sequenceDiagram", diagram);
        Assert.Contains("O->>P: Plan", diagram);
        Assert.Contains("O->>C: Code", diagram);
    }

    [Fact]
    public void ToAsciiFlow_GeneratesReadableChart()
    {
        // Arrange
        var graph = new AgentCallGraph();
        graph.RecordCall(AgentRole.Orchestrator, AgentRole.Planner, "Planning phase", DateTimeOffset.UtcNow);

        // Act
        var ascii = graph.ToAsciiFlow();

        // Assert
        Assert.Contains("ORCHESTRATOR", ascii);
        Assert.Contains("PLANNER", ascii);
        Assert.Contains("[Planning phase]", ascii);
    }
}