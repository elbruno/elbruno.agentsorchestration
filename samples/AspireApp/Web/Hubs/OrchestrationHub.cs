using ElBruno.AgentsOrchestration.Orchestration;
using Microsoft.AspNetCore.SignalR;

namespace AgentsOrchestration.Web.Hubs;

public sealed class OrchestrationHub : Hub
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, CancellationTokenSource> Cancellations = [];
    private readonly IServiceScopeFactory _scopeFactory;

    public OrchestrationHub(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task StartOrchestration(string prompt)
    {
        await CancelOrchestration();

        var cts = new CancellationTokenSource();
        Cancellations[Context.ConnectionId] = cts;
        await using var scope = _scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<OrchestrationService>();

        try
        {
            var readEvents = Task.Run(async () =>
            {
                await foreach (var evt in service.Events.Reader.ReadAllAsync(cts.Token))
                {
                    await Clients.Caller.SendAsync(
                        "OrchestrationEvent",
                        new
                        {
                            Type = evt.GetType().Name,
                            Message = ToMessage(evt),
                            Timestamp = evt.Timestamp
                        },
                        cts.Token);
                }
            }, cts.Token);

            await service.RunAsync(new OrchestrationRequest(prompt), cts.Token);
            service.Events.Writer.TryComplete();
            await readEvents;
        }
        catch (OperationCanceledException)
        {
            await Clients.Caller.SendAsync("OrchestrationCancelled", cts.Token);
        }
        finally
        {
            Cancellations.TryRemove(Context.ConnectionId, out _);
        }
    }

    public Task CancelOrchestration()
    {
        if (Cancellations.TryRemove(Context.ConnectionId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }

        return Task.CompletedTask;
    }

    private static string ToMessage(OrchestrationEvent evt) =>
        evt switch
        {
            OrchestrationStartedEvent started => $"Started: {started.Prompt}",
            PhaseStartedEvent phaseStarted => $"Phase {phaseStarted.PhaseIndex} started: {phaseStarted.PhaseName}",
            AgentActivatedEvent activated => $"{activated.Role} active: {activated.TaskDescription}",
            AgentStreamingEvent streaming => $"{streaming.Role}: {streaming.DeltaContent}",
            AgentInstructionUpdateEvent instruction => $"{instruction.Role} instruction updated",
            AgentCompletedEvent completed => $"{completed.Role} completed",
            PhaseCompletedEvent completedPhase => $"Phase {completedPhase.PhaseIndex} completed",
            FileCreatedEvent file => $"File updated: {file.FilePath}",
            BuildValidationEvent build => build.Success ? $"\u2705 Build succeeded" : $"\u274c Build failed: {build.Output}",
            FixAttemptStartedEvent fixStart => $"\U0001f527 Fix attempt {fixStart.Attempt} started",
            FixAttemptCompletedEvent fixEnd => fixEnd.Success
                ? $"\u2705 Fix attempt {fixEnd.Attempt} succeeded"
                : $"\u274c Fix attempt {fixEnd.Attempt} failed",
            BuildReviewStartedEvent => $"📊 Build quality review started",
            BuildReviewCompletedEvent review => $"📊 Build review: {review.ReviewFeedback.Split('\n').First()}",
            ResearchRequestedEvent research => $"🔍 {research.RequestingAgent} requested research: {research.Query}",
            ResearchCompletedEvent researchDone => $"✅ Research completed: {researchDone.SourcesFound} sources found in {researchDone.Duration.TotalSeconds:F1}s",
            AgentCommunicationEvent comm => $"🔄 {comm.FromAgent} → {comm.ToAgent}: {comm.Summary}",
            AppLaunchedEvent launched => $"App launched: {launched.WorkspacePath}",
            AppStoppedEvent => "App stopped",
            AppLogEvent log => $"[app] {log.LogLine}",
            OrchestrationCompletedEvent done => done.FinalResult.Summary,
            OrchestrationErrorEvent error => $"Error: {error.Error}",
            _ => "Event received"
        };
}
