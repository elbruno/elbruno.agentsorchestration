using System.Diagnostics;

namespace ElBruno.AgentsOrchestration.Workspace;

public sealed class AppRunner : IDisposable
{
    private Process? _process;

    public bool IsRunning => _process is { HasExited: false };

    public event Action<string>? OnLogReceived;

    public async Task<bool> LaunchAsync(string workspacePath, CancellationToken cancellationToken = default)
    {
        await StopAsync();

        var csprojFiles = Directory.GetFiles(workspacePath, "*.csproj", SearchOption.AllDirectories);
        if (csprojFiles.Length == 0)
            return false;

        var csprojDir = Path.GetDirectoryName(csprojFiles[0])!;

        _process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "run --no-build",
                WorkingDirectory = csprojDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };

        _process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null) OnLogReceived?.Invoke(e.Data);
        };
        _process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null) OnLogReceived?.Invoke($"[stderr] {e.Data}");
        };

        _process.Start();
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        // Give the process a moment to start or fail immediately
        await Task.Delay(500, cancellationToken);

        return IsRunning;
    }

    public Task StopAsync()
    {
        if (_process is { HasExited: false })
        {
            try
            {
                _process.Kill(entireProcessTree: true);
                _process.WaitForExit(3000);
            }
            catch (InvalidOperationException)
            {
                // Process already exited
            }
        }

        _process?.Dispose();
        _process = null;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_process is { HasExited: false })
        {
            try { _process.Kill(entireProcessTree: true); }
            catch (InvalidOperationException) { }
        }
        _process?.Dispose();
    }
}
