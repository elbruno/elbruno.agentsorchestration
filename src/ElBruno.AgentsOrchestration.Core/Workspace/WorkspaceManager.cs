using ElBruno.AgentsOrchestration.Orchestration;

namespace ElBruno.AgentsOrchestration.Workspace;

public sealed class WorkspaceManager : IWorkspace
{
    private readonly string _rootPath;
    private static readonly char[] _invalidFileNameChars = ['<', '>', ':', '"', '|', '?', '*', '\\', '/', '\0'];

    public WorkspaceManager(string rootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath, nameof(rootPath));
        _rootPath = Path.GetFullPath(rootPath);
    }

    public string WorkspacePath { get; private set; } = string.Empty;

    public string CreateWorkspace(string prompt)
    {
        var slug = string.Concat((prompt ?? "run").ToLowerInvariant().Where(c => char.IsLetterOrDigit(c) || c == ' ')).Trim();
        slug = string.Join('-', slug.Split(' ', StringSplitOptions.RemoveEmptyEntries)).Trim('-');
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = "orchestration";
        }

        var dir = Path.Combine(_rootPath, $"{DateTime.UtcNow:yyyyMMddHHmmss}-{slug[..Math.Min(slug.Length, 30)]}");
        Directory.CreateDirectory(dir);
        WorkspacePath = dir;
        return dir;
    }

    public IReadOnlyCollection<string> ListFiles()
    {
        if (string.IsNullOrWhiteSpace(WorkspacePath) || !Directory.Exists(WorkspacePath))
        {
            return [];
        }

        return Directory
            .GetFiles(WorkspacePath, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(WorkspacePath, path))
            .OrderBy(path => path)
            .ToArray();
    }

    public string ReadFile(string relativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath, nameof(relativePath));
        
        // Reject paths with ".." to prevent directory traversal
        if (relativePath.Contains("..", StringComparison.Ordinal))
        {
            throw new ArgumentException("Path cannot contain '..' segments", nameof(relativePath));
        }
        
        // Reject absolute paths
        if (Path.IsPathRooted(relativePath))
        {
            throw new ArgumentException("Path must be relative", nameof(relativePath));
        }

        var fullPath = Path.GetFullPath(Path.Combine(WorkspacePath, relativePath));
        if (!fullPath.StartsWith(Path.GetFullPath(WorkspacePath), StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        return File.Exists(fullPath) ? File.ReadAllText(fullPath) : string.Empty;
    }
}
