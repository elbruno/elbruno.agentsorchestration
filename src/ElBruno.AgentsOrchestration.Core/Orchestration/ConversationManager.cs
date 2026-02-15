using System.Text.Json;
using ElBruno.AgentsOrchestration.Orchestration;

namespace ElBruno.AgentsOrchestration.Core.Orchestration;

/// <summary>
/// Manages multi-turn conversation sessions with workspace isolation.
/// Tracks conversation history and builds context prompts for orchestration.
/// Thread-safe for concurrent access.
/// </summary>
public sealed class ConversationManager
{
    private readonly Dictionary<string, ConversationSession> _sessions = new();
    private readonly object _lock = new();

    /// <summary>
    /// Creates a new conversation session for a workspace.
    /// </summary>
    /// <param name="workspacePath">Workspace directory path, must not be null or empty</param>
    /// <returns>New conversation session with unique ID</returns>
    /// <exception cref="ArgumentException">Thrown if workspacePath is null or empty</exception>
    public ConversationSession CreateSession(string workspacePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workspacePath, nameof(workspacePath));

        var sessionId = Guid.NewGuid().ToString("N")[..8];
        var session = new ConversationSession(
            sessionId,
            workspacePath,
            [],
            DateTimeOffset.UtcNow
        );

        lock (_lock)
        {
            _sessions[sessionId] = session;
        }

        return session;
    }

    /// <summary>
    /// Retrieves a session by ID, or null if not found.
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <returns>ConversationSession if found, null otherwise</returns>
    /// <exception cref="ArgumentException">Thrown if sessionId is null or empty</exception>
    public ConversationSession? GetSession(string sessionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId, nameof(sessionId));

        lock (_lock)
        {
            _sessions.TryGetValue(sessionId, out var session);
            return session;
        }
    }

    /// <summary>
    /// Updates a session with a new turn, returning the updated session.
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <param name="userPrompt">User's prompt text</param>
    /// <param name="result">Orchestration result from the turn</param>
    /// <returns>Updated ConversationSession with new turn appended</returns>
    /// <exception cref="ArgumentException">Thrown if sessionId or userPrompt is null/empty, or result is null</exception>
    /// <exception cref="KeyNotFoundException">Thrown if session does not exist</exception>
    public ConversationSession RecordTurn(string sessionId, string userPrompt, OrchestrationResult result)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId, nameof(sessionId));
        ArgumentException.ThrowIfNullOrWhiteSpace(userPrompt, nameof(userPrompt));
        ArgumentNullException.ThrowIfNull(result, nameof(result));

        lock (_lock)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
                throw new KeyNotFoundException($"Session '{sessionId}' does not exist. Create a new session first.");

            var updated = session.AddTurn(userPrompt, result);
            _sessions[sessionId] = updated;
            return updated;
        }
    }

    /// <summary>
    /// Lists all active sessions.
    /// </summary>
    /// <returns>Readonly list of all sessions</returns>
    public IReadOnlyList<ConversationSession> ListSessions()
    {
        lock (_lock)
        {
            return _sessions.Values.ToList();
        }
    }

    /// <summary>
    /// Clears a session from memory.
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <exception cref="ArgumentException">Thrown if sessionId is null or empty</exception>
    public void ClearSession(string sessionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId, nameof(sessionId));

        lock (_lock)
        {
            _sessions.Remove(sessionId);
        }
    }
}
