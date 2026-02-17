using ElBruno.AgentsOrchestration.AgentRepository.Models;

namespace ElBruno.AgentsOrchestration.AgentRepository.Services;

/// <summary>
/// Loads agent definitions from the Awesome Copilot Repository or local cache.
/// </summary>
public sealed class AwesomeCopilotAgentLoader
{
    private readonly HttpClient _httpClient;
    private readonly AgentDefinitionParser _parser;
    private readonly AgentRepositoryOptions _options;
    private readonly string? _cacheDirectory;

    public AwesomeCopilotAgentLoader(
        AgentRepositoryOptions? options = null,
        HttpClient? httpClient = null)
    {
        _options = options ?? new AgentRepositoryOptions();
        _parser = new AgentDefinitionParser();
        _httpClient = httpClient ?? CreateDefaultHttpClient();
        
        if (!string.IsNullOrWhiteSpace(_options.CacheDirectory))
        {
            _cacheDirectory = _options.CacheDirectory;
            Directory.CreateDirectory(_cacheDirectory);
        }
    }

    /// <summary>
    /// Loads an agent definition by its agent ID (filename without extension).
    /// Example: "WinFormsExpert" loads WinFormsExpert.agent.md
    /// </summary>
    public async Task<DynamicAgentDefinition> LoadAgentAsync(string agentId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentId);

        // Check cache first
        if (_cacheDirectory != null)
        {
            var cachedContent = TryLoadFromCache(agentId);
            if (cachedContent != null)
            {
                return _parser.Parse(cachedContent, agentId);
            }
        }

        // Download from repository
        var fileName = agentId.EndsWith(".agent.md", StringComparison.OrdinalIgnoreCase)
            ? agentId
            : $"{agentId}.agent.md";

        var url = $"{_options.RepositoryUrl.TrimEnd('/')}/{fileName}";

        try
        {
            var content = await _httpClient.GetStringAsync(url, cancellationToken);

            // Cache the content
            if (_cacheDirectory != null)
            {
                await SaveToCacheAsync(agentId, content, cancellationToken);
            }

            return _parser.Parse(content, agentId);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"Failed to load agent '{agentId}' from {url}. Ensure the agent exists in the repository.", ex);
        }
    }

    /// <summary>
    /// Loads an agent definition from a direct URL.
    /// </summary>
    public async Task<DynamicAgentDefinition> LoadAgentFromUrlAsync(string url, string? agentId = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        agentId ??= ExtractAgentIdFromUrl(url);

        try
        {
            var content = await _httpClient.GetStringAsync(url, cancellationToken);
            return _parser.Parse(content, agentId);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to load agent from {url}", ex);
        }
    }

    /// <summary>
    /// Loads an agent definition from a local file.
    /// </summary>
    public async Task<DynamicAgentDefinition> LoadAgentFromFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Agent definition file not found: {filePath}");
        }

        var content = await File.ReadAllTextAsync(filePath, cancellationToken);
        var agentId = Path.GetFileNameWithoutExtension(filePath).Replace(".agent", "");
        return _parser.Parse(content, agentId);
    }

    private string? TryLoadFromCache(string agentId)
    {
        if (_cacheDirectory == null)
        {
            return null;
        }

        var cacheFile = GetCacheFilePath(agentId);
        if (!File.Exists(cacheFile))
        {
            return null;
        }

        // Check if cache is expired
        var fileInfo = new FileInfo(cacheFile);
        var expirationTime = TimeSpan.FromHours(_options.CacheExpirationHours);
        if (DateTime.UtcNow - fileInfo.LastWriteTimeUtc > expirationTime)
        {
            return null;
        }

        return File.ReadAllText(cacheFile);
    }

    private async Task SaveToCacheAsync(string agentId, string content, CancellationToken cancellationToken)
    {
        if (_cacheDirectory == null)
        {
            return;
        }

        var cacheFile = GetCacheFilePath(agentId);
        await File.WriteAllTextAsync(cacheFile, content, cancellationToken);
    }

    private string GetCacheFilePath(string agentId)
    {
        var safeFileName = string.Join("_", agentId.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_cacheDirectory!, $"{safeFileName}.agent.md");
    }

    private static string ExtractAgentIdFromUrl(string url)
    {
        var fileName = url.Split('/').LastOrDefault() ?? "unknown";
        return fileName.Replace(".agent.md", "", StringComparison.OrdinalIgnoreCase);
    }

    private HttpClient CreateDefaultHttpClient()
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds)
        };
        client.DefaultRequestHeaders.Add("User-Agent", "ElBruno.AgentsOrchestration.AgentRepository/1.0");
        return client;
    }
}
