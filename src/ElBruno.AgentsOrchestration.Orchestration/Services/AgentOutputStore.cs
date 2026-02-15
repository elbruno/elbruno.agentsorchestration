using System.Collections.Concurrent;
using ElBruno.AgentsOrchestration.Agents;

namespace ElBruno.AgentsOrchestration.Orchestration;

/// <summary>
/// Stores agent outputs during orchestration sessions for UI access and debugging.
/// Thread-safe singleton service that maintains in-memory history of all agent interactions.
/// </summary>
public sealed class AgentOutputStore
{
    private readonly ConcurrentDictionary<string, List<AgentOutput>> _outputs = new();
    private readonly object _lock = new();

    /// <summary>
    /// Stores an agent's output for a specific session.
    /// </summary>
    public void StoreOutput(string sessionId, AgentRole role, string output, DateTimeOffset timestamp, string? taskDescription = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId, nameof(sessionId));
        ArgumentException.ThrowIfNullOrWhiteSpace(output, nameof(output));

        var agentOutput = new AgentOutput(sessionId, role, output, timestamp, taskDescription);

        lock (_lock)
        {
            if (!_outputs.TryGetValue(sessionId, out var list))
            {
                list = [];
                _outputs[sessionId] = list;
            }
            list.Add(agentOutput);
        }
    }

    /// <summary>
    /// Retrieves all agent outputs for a specific session, ordered by timestamp.
    /// </summary>
    public IReadOnlyList<AgentOutput> GetSessionOutputs(string sessionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId, nameof(sessionId));

        lock (_lock)
        {
            return _outputs.TryGetValue(sessionId, out var list)
                ? list.OrderBy(o => o.Timestamp).ToList()
                : [];
        }
    }

    /// <summary>
    /// Retrieves the most recent output from a specific agent role in a session.
    /// </summary>
    public AgentOutput? GetLatestOutput(string sessionId, AgentRole role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId, nameof(sessionId));

        lock (_lock)
        {
            return _outputs.TryGetValue(sessionId, out var list)
                ? list.Where(o => o.Role == role).OrderByDescending(o => o.Timestamp).FirstOrDefault()
                : null;
        }
    }

    /// <summary>
    /// Retrieves all outputs from a specific agent role across all sessions.
    /// </summary>
    public IReadOnlyList<AgentOutput> GetOutputsByRole(AgentRole role)
    {
        lock (_lock)
        {
            return _outputs.Values
                .SelectMany(list => list)
                .Where(o => o.Role == role)
                .OrderByDescending(o => o.Timestamp)
                .ToList();
        }
    }

    /// <summary>
    /// Clears all outputs for a specific session.
    /// </summary>
    public void ClearSession(string sessionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId, nameof(sessionId));
        _outputs.TryRemove(sessionId, out _);
    }

    /// <summary>
    /// Clears all stored outputs (for testing or memory management).
    /// </summary>
    public void ClearAll() => _outputs.Clear();

    /// <summary>
    /// Gets the count of stored outputs across all sessions.
    /// </summary>
    public int TotalOutputCount
    {
        get
        {
            lock (_lock)
            {
                return _outputs.Values.Sum(list => list.Count);
            }
        }
    }
}

/// <summary>
/// Represents a single agent output with metadata.
/// </summary>
public sealed record AgentOutput(
    string SessionId,
    AgentRole Role,
    string Output,
    DateTimeOffset Timestamp,
    string? TaskDescription = null
);
