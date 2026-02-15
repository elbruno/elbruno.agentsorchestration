using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.Orchestration;
using System.Collections.Concurrent;
using GitHub.Copilot.SDK;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Register Copilot SDK client
builder.Services.AddSingleton<CopilotClient>();

// Register orchestration services with Copilot integration
builder.Services.AddOrchestrationWithCopilot(options =>
{
    var configuredRoot = builder.Configuration["Workspace:RootPath"] ?? "../../workspaces";
    options.WorkspaceRoot = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, configuredRoot));
    options.MaxFixAttempts = builder.Configuration.GetValue("Orchestration:MaxFixAttempts", 3);
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Shared state for tracking running orchestrations
var runningOrchestrations = new ConcurrentDictionary<string, OrchestrationRun>();

// ──────────────────────────────────────
// Agent Configuration Endpoints
// ──────────────────────────────────────

var agents = app.MapGroup("/agents");

agents.MapGet("/", (AgentConfigurationStore store) =>
    Results.Ok(store.GetAll()));

agents.MapGet("/{role}", (AgentRole role, AgentConfigurationStore store) =>
    Results.Ok(store.Get(role)));

agents.MapPut("/{role}", (AgentRole role, AgentConfigurationUpdateRequest request, AgentConfigurationStore store) =>
{
    var current = store.Get(role);
    store.Update(current with { Instructions = request.Instructions });
    return Results.Ok(store.Get(role));
});

agents.MapPost("/{role}/reset", (AgentRole role, AgentConfigurationStore store) =>
{
    store.ResetInstructions(role);
    return Results.Ok(store.Get(role));
});

// ──────────────────────────────────────
// Orchestration Endpoints
// ──────────────────────────────────────

app.MapPost("/orchestration/run", async (OrchestrationRunRequest request, IServiceScopeFactory scopeFactory) =>
{
    // Create a new scope for this orchestration run
    var scope = scopeFactory.CreateScope();
    var service = scope.ServiceProvider.GetRequiredService<OrchestrationService>();

    var runId = Guid.NewGuid().ToString("N")[..8];
    var cts = new CancellationTokenSource();
    var run = new OrchestrationRun(runId, service, cts, scope);
    runningOrchestrations[runId] = run;

    try
    {
        var result = await service.RunAsync(new OrchestrationRequest(request.Prompt), cts.Token);
        run.Result = result;
        run.Status = "completed";
        return Results.Ok(new { RunId = runId, Status = "completed", Result = result });
    }
    catch (OperationCanceledException)
    {
        run.Status = "cancelled";
        return Results.Ok(new { RunId = runId, Status = "cancelled" });
    }
    catch (Exception ex)
    {
        run.Status = "failed";
        run.Error = ex.Message;
        return Results.Ok(new { RunId = runId, Status = "failed", Error = ex.Message });
    }
    finally
    {
        scope.Dispose();
    }
});

app.MapGet("/orchestration/status", () =>
{
    var runs = runningOrchestrations.Values.Select(r => new
    {
        r.RunId,
        r.Status,
        r.StartedAt,
        HasResult = r.Result is not null,
        r.Error
    });
    return Results.Ok(runs);
});

app.MapPost("/orchestration/cancel", () =>
{
    var cancelled = new List<string>();
    foreach (var kvp in runningOrchestrations)
    {
        if (kvp.Value.Status == "running")
        {
            kvp.Value.Cts.Cancel();
            kvp.Value.Status = "cancelled";
            cancelled.Add(kvp.Key);
        }
    }
    return Results.Ok(new { CancelledRuns = cancelled });
});

app.Run();

// ──────────────────────────────────────
// Request DTOs & Run Tracking
// ──────────────────────────────────────

sealed record OrchestrationRunRequest(string Prompt);
sealed record AgentConfigurationUpdateRequest(string Instructions);

sealed class OrchestrationRun(string runId, OrchestrationService service, CancellationTokenSource cts, IServiceScope scope)
{
    public string RunId { get; } = runId;
    public OrchestrationService Service { get; } = service;
    public CancellationTokenSource Cts { get; } = cts;
    public IServiceScope Scope { get; } = scope;
    public string Status { get; set; } = "running";
    public DateTimeOffset StartedAt { get; } = DateTimeOffset.UtcNow;
    public OrchestrationResult? Result { get; set; }
    public string? Error { get; set; }
}
