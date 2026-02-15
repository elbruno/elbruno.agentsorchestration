using System.Threading.Channels;
using ElBruno.AgentsOrchestration.Agents;

namespace ElBruno.AgentsOrchestration.Orchestration;

public sealed class OrchestrationService
{
    private readonly AgentFactory _agentFactory;
    private readonly IWorkspace _workspace;
    private readonly int _maxFixAttempts;

    public OrchestrationService(AgentFactory agentFactory, IWorkspace workspace, int maxFixAttempts = 3)
    {
        _agentFactory = agentFactory;
        _workspace = workspace;
        _maxFixAttempts = Math.Clamp(maxFixAttempts, 0, 10);
    }

    public Channel<OrchestrationEvent> Events { get; } = Channel.CreateBounded<OrchestrationEvent>(
        new BoundedChannelOptions(1000) { FullMode = BoundedChannelFullMode.DropOldest });

    public async Task<OrchestrationResult> RunAsync(OrchestrationRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Prompt, nameof(request.Prompt));

        await PublishAsync(new OrchestrationStartedEvent(DateTimeOffset.UtcNow, request.Prompt), cancellationToken);
        try
        {
            var workspacePath = _workspace.CreateWorkspace(request.Prompt);
            var plan = await BuildPlanAsync(request.Prompt, workspacePath, cancellationToken);
            var results = new List<TaskResult>();

            for (var i = 0; i < plan.Phases.Count; i++)
            {
                var phase = plan.Phases.ElementAt(i);
                await PublishAsync(new PhaseStartedEvent(DateTimeOffset.UtcNow, i + 1, phase.Name), cancellationToken);

                var taskResults = await Task.WhenAll(
                    phase.Tasks.Select(task => ExecuteTaskAsync(task, workspacePath, cancellationToken)));
                results.AddRange(taskResults);

                await PublishAsync(new PhaseCompletedEvent(DateTimeOffset.UtcNow, i + 1), cancellationToken);
            }

            var summary = await VerifyAsync(results, workspacePath, cancellationToken);
            var review = await ReviewAsync(summary, workspacePath, cancellationToken);
            var finalResult = new OrchestrationResult(review, results, workspacePath);
            await PublishAsync(new OrchestrationCompletedEvent(DateTimeOffset.UtcNow, finalResult), cancellationToken);
            return finalResult;
        }
        catch (Exception ex)
        {
            await PublishAsync(new OrchestrationErrorEvent(DateTimeOffset.UtcNow, ex.Message), cancellationToken);
            throw;
        }
    }

    private async Task<ExecutionPlan> BuildPlanAsync(string prompt, string workspacePath, CancellationToken cancellationToken)
    {
        var planner = _agentFactory.CreateSession(AgentRole.Planner);
        await PublishAsync(new AgentActivatedEvent(DateTimeOffset.UtcNow, AgentRole.Planner, "Building execution plan", planner.Configuration.Instructions.Split('\n').FirstOrDefault() ?? ""), cancellationToken);

        var plannerOutput = await planner.RunAsync(prompt, workspacePath, cancellationToken);
        await PublishAsync(new AgentInstructionUpdateEvent(DateTimeOffset.UtcNow, AgentRole.Planner, plannerOutput), cancellationToken);
        await PublishAsync(new AgentCompletedEvent(DateTimeOffset.UtcNow, AgentRole.Planner, plannerOutput), cancellationToken);

        return ParsePlan(plannerOutput, prompt);
    }

    internal static ExecutionPlan ParsePlan(string plannerOutput, string prompt)
    {
        var phases = new List<ExecutionPhase>();
        var currentPhaseName = (string?)null;
        var currentTasks = new List<ExecutionTask>();

        foreach (var rawLine in plannerOutput.Split('\n'))
        {
            var line = rawLine.Trim();

            if (line.StartsWith("## ") && line.Contains("Phase", StringComparison.OrdinalIgnoreCase))
            {
                if (currentPhaseName is not null && currentTasks.Count > 0)
                {
                    phases.Add(new ExecutionPhase(currentPhaseName, currentTasks.ToArray()));
                    currentTasks.Clear();
                }

                currentPhaseName = line[3..].Trim();
                continue;
            }

            if (line.StartsWith("- Task:") && currentPhaseName is not null)
            {
                var parsed = ParseTaskLine(line, prompt);
                if (parsed is not null)
                {
                    currentTasks.Add(parsed);
                }
            }
        }

        if (currentPhaseName is not null && currentTasks.Count > 0)
        {
            phases.Add(new ExecutionPhase(currentPhaseName, currentTasks.ToArray()));
        }

        if (phases.Count == 0)
        {
            phases.Add(new ExecutionPhase("Implementation",
            [
                new ExecutionTask($"Implement main application logic for: {prompt}", AgentRole.Coder, "Program.cs"),
                new ExecutionTask($"Create data models for: {prompt}", AgentRole.Coder, "Models.cs"),
                new ExecutionTask($"Create application styles for: {prompt}", AgentRole.Designer, "styles.css")
            ]));
        }

        return new ExecutionPlan(phases);
    }

    private static ExecutionTask? ParseTaskLine(string line, string prompt)
    {
        // Format: "- Task: <description> | Agent: <role> | File: <path>"
        var parts = line[2..].Split('|').Select(p => p.Trim()).ToArray();
        if (parts.Length < 3)
        {
            return null;
        }

        var description = parts[0].StartsWith("Task:", StringComparison.OrdinalIgnoreCase)
            ? parts[0][5..].Trim()
            : parts[0];
        description = $"{description} for: {prompt}";

        var agentStr = parts[1].StartsWith("Agent:", StringComparison.OrdinalIgnoreCase)
            ? parts[1][6..].Trim()
            : parts[1];
        var role = Enum.TryParse<AgentRole>(agentStr, ignoreCase: true, out var parsed)
            ? parsed
            : AgentRole.Coder;

        var file = parts[2].StartsWith("File:", StringComparison.OrdinalIgnoreCase)
            ? parts[2][5..].Trim()
            : parts[2];

        return new ExecutionTask(description, role, file);
    }

    private async Task<TaskResult> ExecuteTaskAsync(ExecutionTask task, string workspacePath, CancellationToken cancellationToken)
    {
        var session = _agentFactory.CreateSession(task.AssignedRole);
        var instructionPreview = string.Join('\n', session.Configuration.Instructions.Split('\n').Take(2));
        await PublishAsync(new AgentActivatedEvent(DateTimeOffset.UtcNow, task.AssignedRole, task.Description, instructionPreview), cancellationToken);

        var output = await session.RunAsync(task.Description, workspacePath, cancellationToken);
        await PublishAsync(new AgentStreamingEvent(DateTimeOffset.UtcNow, task.AssignedRole, output), cancellationToken);

        var fullPath = Path.GetFullPath(Path.Combine(workspacePath, task.FileScope.Replace('/', Path.DirectorySeparatorChar)));
        var normalizedWorkspace = Path.GetFullPath(workspacePath);
        if (!fullPath.StartsWith(normalizedWorkspace, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"File scope '{task.FileScope}' resolves outside the workspace directory.");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllTextAsync(fullPath, output, cancellationToken);
        await PublishAsync(new FileCreatedEvent(DateTimeOffset.UtcNow, task.FileScope), cancellationToken);
        await PublishAsync(new AgentCompletedEvent(DateTimeOffset.UtcNow, task.AssignedRole, output), cancellationToken);

        return new TaskResult(task.AssignedRole, task.Description, output);
    }

    private async Task<string> VerifyAsync(IReadOnlyCollection<TaskResult> results, string workspacePath, CancellationToken cancellationToken)
    {
        // Run dotnet build to validate generated code
        var (buildSuccess, buildOutput) = await RunBuildValidationAsync(workspacePath, cancellationToken);

        // Fix loop: if build fails, invoke the Fixer agent to correct errors
        var attempt = 0;
        while (!buildSuccess && attempt < _maxFixAttempts)
        {
            attempt++;
            await PublishAsync(new FixAttemptStartedEvent(DateTimeOffset.UtcNow, attempt, buildOutput), cancellationToken);

            var fixer = _agentFactory.CreateSession(AgentRole.Fixer);
            var instructionPreview = string.Join('\n', fixer.Configuration.Instructions.Split('\n').Take(2));
            await PublishAsync(new AgentActivatedEvent(DateTimeOffset.UtcNow, AgentRole.Fixer, $"Fix attempt {attempt}/{_maxFixAttempts}", instructionPreview), cancellationToken);

            var fixPrompt = $"Build failed. Fix the following errors:\n{buildOutput}\n\nWorkspace: {workspacePath}";
            var fixOutput = await fixer.RunAsync(fixPrompt, workspacePath, cancellationToken);
            await PublishAsync(new AgentStreamingEvent(DateTimeOffset.UtcNow, AgentRole.Fixer, fixOutput), cancellationToken);

            // Write fixer output — the template client returns corrected file content
            // In a real LLM scenario the fixer would specify which files to update
            var csprojFiles = Directory.GetFiles(workspacePath, "*.csproj", SearchOption.AllDirectories);
            if (csprojFiles.Length > 0 && fixOutput.Contains("<Project", StringComparison.OrdinalIgnoreCase))
            {
                await File.WriteAllTextAsync(csprojFiles[0], fixOutput, cancellationToken);
                await PublishAsync(new FileCreatedEvent(DateTimeOffset.UtcNow, Path.GetRelativePath(workspacePath, csprojFiles[0])), cancellationToken);
            }

            await PublishAsync(new AgentCompletedEvent(DateTimeOffset.UtcNow, AgentRole.Fixer, fixOutput), cancellationToken);

            // Re-run build validation
            (buildSuccess, buildOutput) = await RunBuildValidationAsync(workspacePath, cancellationToken);

            await PublishAsync(new FixAttemptCompletedEvent(DateTimeOffset.UtcNow, attempt, buildSuccess, buildOutput), cancellationToken);
        }

        var orchestrator = _agentFactory.CreateSession(AgentRole.Orchestrator);
        var buildStatus = buildSuccess ? "BUILD SUCCEEDED" : $"BUILD FAILED after {attempt} fix attempt(s)";
        var prompt = $"Workspace: {workspacePath}{Environment.NewLine}Build: {buildStatus}{Environment.NewLine}Results: {string.Join("; ", results.Select(r => r.Description))}";
        return await orchestrator.RunAsync(prompt, workspacePath, cancellationToken);
    }

    private async Task<string> ReviewAsync(string verificationSummary, string workspacePath, CancellationToken cancellationToken)
    {
        // Run the BuildReviewer agent to analyze code quality and provide feedback
        // Get the final build output for analysis
        var (buildSuccess, buildOutput) = await RunBuildValidationAsync(workspacePath, cancellationToken);

        // Only run review if build succeeded (no point reviewing broken code)
        if (!buildSuccess)
        {
            return verificationSummary;
        }

        await PublishAsync(new BuildReviewStartedEvent(DateTimeOffset.UtcNow, buildOutput), cancellationToken);

        var reviewer = _agentFactory.CreateSession(AgentRole.BuildReviewer);
        var instructionPreview = string.Join('\n', reviewer.Configuration.Instructions.Split('\n').Take(2));
        await PublishAsync(new AgentActivatedEvent(DateTimeOffset.UtcNow, AgentRole.BuildReviewer, "Reviewing build quality and best practices", instructionPreview), cancellationToken);

        var reviewPrompt = $@"Build succeeded. Review the generated code for quality, performance, and best practices.

Build Output:
{buildOutput}

Workspace: {workspacePath}

Provide specific, actionable feedback on code quality, security, performance, and .NET best practices.";

        var reviewFeedback = await reviewer.RunAsync(reviewPrompt, workspacePath, cancellationToken);
        await PublishAsync(new AgentStreamingEvent(DateTimeOffset.UtcNow, AgentRole.BuildReviewer, reviewFeedback), cancellationToken);
        await PublishAsync(new AgentCompletedEvent(DateTimeOffset.UtcNow, AgentRole.BuildReviewer, reviewFeedback), cancellationToken);
        await PublishAsync(new BuildReviewCompletedEvent(DateTimeOffset.UtcNow, reviewFeedback), cancellationToken);

        // Combine verification summary and review feedback
        var combinedSummary = $@"{verificationSummary}

---

## Quality Review

{reviewFeedback}";

        return combinedSummary;
    }

    private async Task<(bool Success, string Output)> RunBuildValidationAsync(string workspacePath, CancellationToken cancellationToken)
    {
        // Look for a .csproj file in the workspace
        var csprojFiles = Directory.GetFiles(workspacePath, "*.csproj", SearchOption.AllDirectories);
        if (csprojFiles.Length == 0)
        {
            var msg = "No .csproj file found in workspace. Skipping build validation.";
            await PublishAsync(new BuildValidationEvent(DateTimeOffset.UtcNow, false, msg), cancellationToken);
            return (false, msg);
        }

        var csprojDir = Path.GetDirectoryName(csprojFiles[0])!;
        try
        {
            using var process = new System.Diagnostics.Process();
            process.StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "build --verbosity quiet",
                WorkingDirectory = csprojDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start();

            var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            var success = process.ExitCode == 0;
            var output = success
                ? $"Build succeeded.\n{stdout}".Trim()
                : $"Build failed (exit code {process.ExitCode}).\n{stdout}\n{stderr}".Trim();

            // Truncate very long output
            if (output.Length > 2000)
                output = $"{output[..2000]}\n... (truncated)";

            await PublishAsync(new BuildValidationEvent(DateTimeOffset.UtcNow, success, output), cancellationToken);
            return (success, output);
        }
        catch (Exception ex)
        {
            var msg = $"Build validation failed to run: {ex.Message}";
            await PublishAsync(new BuildValidationEvent(DateTimeOffset.UtcNow, false, msg), cancellationToken);
            return (false, msg);
        }
    }

    private ValueTask PublishAsync(OrchestrationEvent orchestrationEvent, CancellationToken cancellationToken) =>
        Events.Writer.WriteAsync(orchestrationEvent, cancellationToken);
}
