using ElBruno.AgentsOrchestration.AgentRepository.Models;
using ElBruno.AgentsOrchestration.AgentRepository.Services;
using Xunit;

namespace AgentsOrchestration.AgentRepository.Tests;

public sealed class AwesomeCopilotAgentLoaderTests
{
    [Fact]
    public async Task LoadAgentFromFileAsync_WithValidFile_ReturnsDefinition()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var testFile = Path.Combine(tempDir, "TestAgent.agent.md");
            var content = @"---
name: Test Agent
description: A test agent
---

# Test Instructions
This is a test.
";
            await File.WriteAllTextAsync(testFile, content);

            var loader = new AwesomeCopilotAgentLoader();

            // Act
            var result = await loader.LoadAgentFromFileAsync(testFile);

            // Assert
            Assert.Equal("Test Agent", result.Name);
            Assert.Equal("A test agent", result.Description);
            Assert.Contains("# Test Instructions", result.Instructions);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task LoadAgentFromFileAsync_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var loader = new AwesomeCopilotAgentLoader();
        var nonExistentFile = Path.Combine(Path.GetTempPath(), "does-not-exist.agent.md");

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => loader.LoadAgentFromFileAsync(nonExistentFile));
    }

    [Fact]
    public void Constructor_WithCacheDirectory_CreatesDirectory()
    {
        // Arrange
        var cacheDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            var options = new AgentRepositoryOptions { CacheDirectory = cacheDir };

            // Act
            var loader = new AwesomeCopilotAgentLoader(options);

            // Assert
            Assert.True(Directory.Exists(cacheDir));
        }
        finally
        {
            if (Directory.Exists(cacheDir))
            {
                Directory.Delete(cacheDir, true);
            }
        }
    }
}
