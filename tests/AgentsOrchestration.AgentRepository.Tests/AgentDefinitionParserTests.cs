using ElBruno.AgentsOrchestration.AgentRepository.Services;
using Xunit;

namespace AgentsOrchestration.AgentRepository.Tests;

public sealed class AgentDefinitionParserTests
{
    [Fact]
    public void Parse_WithFullFrontMatter_ReturnsCorrectDefinition()
    {
        // Arrange
        var parser = new AgentDefinitionParser();
        var content = @"---
name: Test Agent
description: A test agent for unit testing
model: gpt-5
version: 1.0
tools: ['codebase', 'search']
---

# Test Agent Instructions

This is a test agent with full instructions.
";

        // Act
        var result = parser.Parse(content, "test-agent");

        // Assert
        Assert.Equal("Test Agent", result.Name);
        Assert.Equal("A test agent for unit testing", result.Description);
        Assert.Equal("gpt-5", result.Model);
        Assert.Equal("1.0", result.Version);
        Assert.NotNull(result.Tools);
        Assert.Contains("codebase", result.Tools);
        Assert.Contains("search", result.Tools);
        Assert.Contains("# Test Agent Instructions", result.Instructions);
    }

    [Fact]
    public void Parse_WithoutFrontMatter_TreatsEntireContentAsInstructions()
    {
        // Arrange
        var parser = new AgentDefinitionParser();
        var content = "This is just plain markdown content without front matter.";

        // Act
        var result = parser.Parse(content, "simple-agent");

        // Assert
        Assert.Equal("simple-agent", result.Name);
        Assert.Equal(content, result.Instructions);
    }

    [Fact]
    public void Parse_WithQuotedValues_RemovesQuotes()
    {
        // Arrange
        var parser = new AgentDefinitionParser();
        var content = @"---
name: ""Quoted Agent""
description: 'Single quoted description'
---

Instructions here.
";

        // Act
        var result = parser.Parse(content);

        // Assert
        Assert.Equal("Quoted Agent", result.Name);
        Assert.Equal("Single quoted description", result.Description);
    }

    [Fact]
    public void Parse_WithNullContent_ThrowsArgumentException()
    {
        // Arrange
        var parser = new AgentDefinitionParser();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => parser.Parse(null!));
    }
}
