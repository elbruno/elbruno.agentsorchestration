using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.AgentRepository.Services;
using Xunit;

namespace AgentsOrchestration.AgentRepository.Tests;

public sealed class DynamicAgentManagerTests
{
    [Fact]
    public void GetAllAgents_ReturnsStaticAgents()
    {
        // Arrange
        var staticStore = new AgentConfigurationStore();
        var loader = new AwesomeCopilotAgentLoader();
        var manager = new DynamicAgentManager(staticStore, loader);

        // Act
        var agents = manager.GetAllAgents();

        // Assert
        Assert.NotEmpty(agents);
        Assert.Equal(11, agents.Count); // 11 static agents
    }

    [Fact]
    public async Task LoadAndRegisterAgentAsync_WithValidAgent_ReturnsConfiguration()
    {
        // Arrange
        var staticStore = new AgentConfigurationStore();
        var loader = new AwesomeCopilotAgentLoader();
        var manager = new DynamicAgentManager(staticStore, loader);

        // Act - Load WinForms Expert
        var result = await manager.LoadAndRegisterAgentAsync("WinFormsExpert");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("WinForms Expert", result.Name);
        Assert.NotEmpty(result.Instructions);
        Assert.Contains("WinForms", result.Instructions);
    }

    [Fact]
    public async Task LoadAndRegisterAgentAsync_IncreasesTotalAgentCount()
    {
        // Arrange
        var staticStore = new AgentConfigurationStore();
        var loader = new AwesomeCopilotAgentLoader();
        var manager = new DynamicAgentManager(staticStore, loader);
        var initialCount = manager.GetAllAgents().Count;

        // Act
        await manager.LoadAndRegisterAgentAsync("WinFormsExpert");

        // Assert
        var newCount = manager.GetAllAgents().Count;
        Assert.Equal(initialCount + 1, newCount);
    }

    [Fact]
    public async Task GetByName_WithDynamicAgent_ReturnsConfiguration()
    {
        // Arrange
        var staticStore = new AgentConfigurationStore();
        var loader = new AwesomeCopilotAgentLoader();
        var manager = new DynamicAgentManager(staticStore, loader);
        await manager.LoadAndRegisterAgentAsync("WinFormsExpert");

        // Act
        var result = manager.GetByName("WinForms Expert");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("WinForms Expert", result.Name);
    }

    [Fact]
    public async Task GetByName_WithStaticAgent_ReturnsConfiguration()
    {
        // Arrange
        var staticStore = new AgentConfigurationStore();
        var loader = new AwesomeCopilotAgentLoader();
        var manager = new DynamicAgentManager(staticStore, loader);

        // Act
        var result = manager.GetByName("Coder");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Coder", result.Name);
    }

    [Fact]
    public async Task HasAgent_WithDynamicAgent_ReturnsTrue()
    {
        // Arrange
        var staticStore = new AgentConfigurationStore();
        var loader = new AwesomeCopilotAgentLoader();
        var manager = new DynamicAgentManager(staticStore, loader);
        await manager.LoadAndRegisterAgentAsync("WinFormsExpert");

        // Act & Assert
        Assert.True(manager.HasAgent("WinForms Expert"));
    }

    [Fact]
    public async Task RemoveDynamicAgent_RemovesAgent()
    {
        // Arrange
        var staticStore = new AgentConfigurationStore();
        var loader = new AwesomeCopilotAgentLoader();
        var manager = new DynamicAgentManager(staticStore, loader);
        await manager.LoadAndRegisterAgentAsync("WinFormsExpert");
        Assert.True(manager.HasAgent("WinForms Expert"));

        // Act
        var removed = manager.RemoveDynamicAgent("WinForms Expert");

        // Assert
        Assert.True(removed);
        Assert.False(manager.HasAgent("WinForms Expert"));
    }

    [Fact]
    public async Task GetDynamicAgentNames_ReturnsOnlyDynamicAgents()
    {
        // Arrange
        var staticStore = new AgentConfigurationStore();
        var loader = new AwesomeCopilotAgentLoader();
        var manager = new DynamicAgentManager(staticStore, loader);

        // Act - Load two dynamic agents
        await manager.LoadAndRegisterAgentAsync("WinFormsExpert");
        await manager.LoadAndRegisterAgentAsync("se-security-reviewer");
        var dynamicNames = manager.GetDynamicAgentNames();

        // Assert
        Assert.Equal(2, dynamicNames.Count);
        Assert.Contains("WinForms Expert", dynamicNames);
        Assert.Contains("SE: Security", dynamicNames);
    }
}
