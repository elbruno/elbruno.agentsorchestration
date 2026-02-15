namespace ElBruno.AgentsOrchestration.Orchestration;

/// <summary>
/// Abstracts workspace operations so the orchestration engine remains
/// independent of any specific file-system or process-management strategy.
/// </summary>
public interface IWorkspace
{
    /// <summary>
    /// Creates a new isolated workspace directory for an orchestration run
    /// and returns its absolute path.
    /// </summary>
    string CreateWorkspace(string prompt);
}
