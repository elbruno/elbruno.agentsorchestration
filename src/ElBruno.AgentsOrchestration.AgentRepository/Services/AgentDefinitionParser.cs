using System.Text;
using System.Text.RegularExpressions;
using ElBruno.AgentsOrchestration.AgentRepository.Models;

namespace ElBruno.AgentsOrchestration.AgentRepository.Services;

/// <summary>
/// Parses agent definition files in the Awesome Copilot format (.agent.md files).
/// Supports YAML front matter and markdown content.
/// </summary>
public sealed class AgentDefinitionParser
{
    private static readonly Regex FrontMatterRegex = new(@"^---\s*\n(.*?)\n---\s*\n(.*)$", 
        RegexOptions.Singleline | RegexOptions.Compiled);

    /// <summary>
    /// Parses an agent definition from markdown content.
    /// </summary>
    /// <param name="content">The markdown content with YAML front matter</param>
    /// <param name="agentId">Optional agent ID for metadata</param>
    /// <returns>A DynamicAgentDefinition</returns>
    public DynamicAgentDefinition Parse(string content, string? agentId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        var match = FrontMatterRegex.Match(content);
        if (!match.Success)
        {
            // No front matter, treat entire content as instructions
            return new DynamicAgentDefinition(
                Name: agentId ?? "Unknown Agent",
                Description: "Agent loaded from repository",
                Instructions: content.Trim(),
                Metadata: agentId != null ? new Dictionary<string, string> { ["AgentId"] = agentId } : null
            );
        }

        var frontMatterText = match.Groups[1].Value;
        var instructions = match.Groups[2].Value.Trim();

        var frontMatter = ParseFrontMatter(frontMatterText);

        var metadata = new Dictionary<string, string>();
        if (agentId != null)
        {
            metadata["AgentId"] = agentId;
        }
        if (frontMatter.Version != null)
        {
            metadata["Version"] = frontMatter.Version;
        }

        return new DynamicAgentDefinition(
            Name: frontMatter.Name,
            Description: frontMatter.Description,
            Instructions: instructions,
            Model: frontMatter.Model,
            Version: frontMatter.Version,
            Tools: frontMatter.Tools,
            Metadata: metadata.Count > 0 ? metadata : null
        );
    }

    private static AgentFrontMatter ParseFrontMatter(string yamlContent)
    {
        var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var tools = new List<string>();

        foreach (var line in yamlContent.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
            {
                continue;
            }

            var colonIndex = trimmed.IndexOf(':');
            if (colonIndex <= 0)
            {
                continue;
            }

            var key = trimmed[..colonIndex].Trim();
            var value = trimmed[(colonIndex + 1)..].Trim();

            // Handle quoted values
            if ((value.StartsWith('"') && value.EndsWith('"')) ||
                (value.StartsWith('\'') && value.EndsWith('\'')))
            {
                value = value[1..^1];
            }

            // Handle array values (tools)
            if (value.StartsWith('[') && value.EndsWith(']'))
            {
                var arrayContent = value[1..^1];
                tools.AddRange(
                    arrayContent.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim().Trim('\'', '"'))
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                );
            }
            else
            {
                properties[key] = value;
            }
        }

        return new AgentFrontMatter(
            Name: properties.GetValueOrDefault("name") ?? "Unknown Agent",
            Description: properties.GetValueOrDefault("description") ?? "No description",
            Model: properties.GetValueOrDefault("model"),
            Version: properties.GetValueOrDefault("version"),
            Tools: tools.Count > 0 ? tools : null
        );
    }
}
