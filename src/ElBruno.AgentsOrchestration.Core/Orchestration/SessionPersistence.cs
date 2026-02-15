using System.Text.Json;
using ElBruno.AgentsOrchestration.Orchestration;

namespace ElBruno.AgentsOrchestration.Core.Orchestration;

/// <summary>
/// Handles serialization and persistence of conversation sessions to/from JSON files.
/// Thread-safe for concurrent file I/O.
/// </summary>
public sealed class SessionPersistence
{
    private readonly string _persistenceRootPath;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Creates a new session persistence service.
    /// </summary>
    /// <param name="persistenceRootPath">Root path where session files will be stored</param>
    /// <exception cref="ArgumentException">Thrown if persistenceRootPath is null or empty</exception>
    public SessionPersistence(string persistenceRootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(persistenceRootPath, nameof(persistenceRootPath));
        _persistenceRootPath = persistenceRootPath;

        // Ensure root path exists
        Directory.CreateDirectory(_persistenceRootPath);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Saves a conversation session to a JSON file.
    /// </summary>
    /// <param name="session">The conversation session to save</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>The file path where the session was saved</returns>
    /// <exception cref="ArgumentNullException">Thrown if session is null</exception>
    public async Task<string> SaveSessionAsync(ConversationSession session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session, nameof(session));

        try
        {
            var fileName = $"session_{session.SessionId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine(_persistenceRootPath, fileName);

            var json = JsonSerializer.Serialize(session, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json, cancellationToken);

            return filePath;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save session '{session.SessionId}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Loads a conversation session from a JSON file.
    /// </summary>
    /// <param name="filePath">Path to the session file</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>The loaded conversation session, or null if file doesn't exist</returns>
    /// <exception cref="ArgumentException">Thrown if filePath is null or empty</exception>
    public async Task<ConversationSession?> LoadSessionAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath, nameof(filePath));

        try
        {
            if (!File.Exists(filePath))
                return null;

            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            return JsonSerializer.Deserialize<ConversationSession>(json, _jsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize session from '{filePath}': {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load session from '{filePath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Lists all saved session files in the persistence root path.
    /// </summary>
    /// <returns>Collection of session file paths</returns>
    public IReadOnlyList<string> ListSavedSessionFiles()
    {
        try
        {
            return Directory.GetFiles(_persistenceRootPath, "session_*.json")
                .OrderByDescending(f => File.GetLastWriteTimeUtc(f))
                .ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to list session files: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Loads all saved sessions from the persistence root path.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>Collection of loaded conversation sessions</returns>
    public async Task<IReadOnlyList<ConversationSession>> LoadAllSessionsAsync(CancellationToken cancellationToken = default)
    {
        var sessions = new List<ConversationSession>();
        var files = ListSavedSessionFiles();

        foreach (var filePath in files)
        {
            try
            {
                var session = await LoadSessionAsync(filePath, cancellationToken);
                if (session is not null)
                {
                    sessions.Add(session);
                }
            }
            catch (Exception ex)
            {
                // Log and continue loading other sessions
                Console.Error.WriteLine($"⚠️ Failed to load session from {filePath}: {ex.Message}");
            }
        }

        return sessions;
    }

    /// <summary>
    /// Deletes a session file.
    /// </summary>
    /// <param name="filePath">Path to the session file to delete</param>
    /// <exception cref="ArgumentException">Thrown if filePath is null or empty</exception>
    public void DeleteSessionFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath, nameof(filePath));

        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete session file '{filePath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Exports a conversation session to a human-readable markdown format.
    /// </summary>
    /// <param name="session">The conversation session to export</param>
    /// <returns>Markdown-formatted conversation transcript</returns>
    public string ExportAsMarkdown(ConversationSession session)
    {
        ArgumentNullException.ThrowIfNull(session, nameof(session));

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"# Conversation Session: {session.SessionId}");
        sb.AppendLine($"\n**Created:** {session.CreatedAt:yyyy-MM-dd HH:mm:ss UTC}");
        sb.AppendLine($"**Workspace:** {session.WorkspacePath}");
        sb.AppendLine($"**Total Turns:** {session.History.Count}\n");
        sb.AppendLine("---\n");

        for (int i = 0; i < session.History.Count; i++)
        {
            var turn = session.History[i];
            sb.AppendLine($"## Turn {i + 1}");
            sb.AppendLine($"\n**User Prompt:**\n\n{turn.UserPrompt}\n");
            sb.AppendLine($"**Assistant Response:**\n\n{turn.Result.Summary}\n");
            sb.AppendLine($"*Timestamp: {turn.Timestamp:yyyy-MM-dd HH:mm:ss UTC}*\n");
            sb.AppendLine("---\n");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Saves a conversation session as markdown.
    /// </summary>
    /// <param name="session">The conversation session to export</param>
    /// <param name="filePath">Path where the markdown file will be saved</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>The file path where the session was saved</returns>
    public async Task<string> SaveAsMarkdownAsync(ConversationSession session, string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session, nameof(session));
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath, nameof(filePath));

        try
        {
            var markdown = ExportAsMarkdown(session);
            await File.WriteAllTextAsync(filePath, markdown, cancellationToken);
            return filePath;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save session as markdown: {ex.Message}", ex);
        }
    }
}
