# Event Streaming Architecture

The ElBruno.AgentsOrchestration system uses **real-time event streaming** to provide live feedback on orchestration progress. All agent interactions, build operations, and system state changes are published as events that can be consumed by UI components, dashboards, APIs, or custom integrations.

## Overview

Event streaming allows you to:

- **Monitor orchestration progress** in real-time
- **Track agent interactions** and their execution flow
- **Respond to specific events** (e.g., when a build fails, when research completes)
- **Build rich dashboards** with live updates (via SignalR, WebSockets, etc.)
- **Implement custom workflows** triggered by events
- **Debug and trace** agent behavior and communication patterns

## Architecture

### Channel-Based Implementation

The event streaming system is built on .NET's `System.Threading.Channels`:

```csharp
public Channel<OrchestrationEvent> Events { get; } = Channel.CreateBounded<OrchestrationEvent>(
    new BoundedChannelOptions(1000) { FullMode = BoundedChannelFullMode.DropOldest });
```

**Key characteristics:**

- **Bounded channel** with capacity of 1000 events
- **DropOldest strategy** — when full, oldest events are discarded to make room for new ones
- **Thread-safe** — multiple readers and one writer can safely access the channel
- **Async-first** — uses `await foreach` for efficient event consumption
- **No external dependencies** — built into .NET runtime

### Publisher

The `OrchestrationService` publishes events during execution:

```csharp
private ValueTask PublishAsync(OrchestrationEvent orchestrationEvent, CancellationToken cancellationToken) =>
    Events.Writer.WriteAsync(orchestrationEvent, cancellationToken);
```

Events are published at key orchestration checkpoints:

- Pipeline steps (Plan, Parse, Execute, Verify, Review, Report)
- Agent activation and completion
- Build validation and fix attempts
- File creation in workspace
- Research requests and completions
- Inter-agent communication

### Consumer Pattern

Consumers read events with `ReadAllAsync()`:

```csharp
await foreach (var evt in service.Events.Reader.ReadAllAsync(cancellationToken))
{
    // Process event
    Console.WriteLine($"[{evt.Timestamp:O}] {evt.GetType().Name}");
}
```

The `ReadAllAsync()` method blocks until events are available and completes when the channel is closed via `Events.Writer.TryComplete()`.

## Event Types (21 Total)

Events are organized into three categories:

### Core Pipeline Events

Events related to the main orchestration pipeline execution:

| Event | Description | Data |
|-------|-------------|------|
| `OrchestrationStartedEvent` | Orchestration begins | User prompt |
| `PhaseStartedEvent` | Execution phase starts | Phase index, name |
| `AgentActivatedEvent` | Agent assigned a task | Agent role, task description, instruction preview |
| `AgentStreamingEvent` | Agent producing output (streaming token) | Agent role, delta content |
| `AgentInstructionUpdateEvent` | Agent instructions updated at runtime | Agent role, new instruction |
| `AgentCompletedEvent` | Agent finishes task | Agent role, result |
| `PhaseCompletedEvent` | Execution phase completes | Phase index |
| `OrchestrationCompletedEvent` | Pipeline completed successfully | Result summary, flow diagram, call graph, statistics |
| `OrchestrationErrorEvent` | Pipeline failed with error | Error message |

### Build & Validation Events

Events related to build validation, error detection, and fixing:

| Event | Description | Data |
|-------|-------------|------|
| `BuildValidationEvent` | Build result after dotnet build | Success flag, build output |
| `FixAttemptStartedEvent` | Fixer agent retry attempt begins | Attempt number, build output |
| `FixAttemptCompletedEvent` | Fixer agent retry attempt completes | Attempt number, success flag, output |
| `BuildReviewStartedEvent` | BuildReviewer begins quality analysis | Build output |
| `BuildReviewCompletedEvent` | BuildReviewer completes analysis | Review feedback |

### File & Workspace Events

Events related to file operations and workspace changes:

| Event | Description | Data |
|-------|-------------|------|
| `FileCreatedEvent` | File written to workspace | File path |
| `AppLaunchedEvent` | Generated app starts as background process | Workspace path |
| `AppStoppedEvent` | Generated app stopped | (No additional data) |
| `AppLogEvent` | Log line from running app | Log line text |

### Research & Communication Events

Events related to research requests and inter-agent communication:

| Event | Description | Data |
|-------|-------------|------|
| `ResearchRequestedEvent` | Agent requests external research | Requesting agent, query, scope |
| `ResearchCompletedEvent` | Research completed with results | Requesting agent, sources found, duration |
| `AgentCommunicationEvent` | Communication between agents | From agent, to agent, communication type, summary |

## Consuming Events

### Console Application Example

```csharp
using ElBruno.AgentsOrchestration.Orchestration;
using ElBruno.AgentsOrchestration.Core.Orchestration;

var service = OrchestrationServiceFactory.Create();

// Start consuming events in background
var eventTask = Task.Run(async () =>
{
    await foreach (var evt in service.Events.Reader.ReadAllAsync())
    {
        Console.WriteLine($"[{evt.Timestamp:HH:mm:ss}] {evt switch
        {
            OrchestrationStartedEvent started => $"🚀 Started: {started.Prompt}",
            AgentActivatedEvent activated => $"🤖 {activated.Role} active: {activated.TaskDescription}",
            BuildValidationEvent build => build.Success ? "✅ Build succeeded" : "❌ Build failed",
            OrchestrationCompletedEvent done => $"✏️ Complete: {done.FinalResult.Summary}",
            _ => evt.GetType().Name
        }}");
    }
});

// Run orchestration
var result = await service.RunAsync(
    new OrchestrationRequest("Create a weather app"),
    CancellationToken.None
);

service.Events.Writer.TryComplete();
await eventTask;
```

### Blazor Component Example

For real-time UI updates in Blazor:

```csharp
@code {
    private List<ActivityItem> Activities { get; } = new();

    private async Task RunOrchestrationAsync(string prompt)
    {
        var readEventsTask = Task.Run(async () =>
        {
            await foreach (var evt in _orchestrationService.Events.Reader.ReadAllAsync(_cts.Token))
            {
                ApplyEvent(evt);
                StateHasChanged(); // Trigger re-render
            }
        }, _cts.Token);

        await _orchestrationService.RunAsync(
            new OrchestrationRequest(prompt),
            _cts.Token
        );

        _orchestrationService.Events.Writer.TryComplete();
        await readEventsTask;
    }

    private void ApplyEvent(OrchestrationEvent evt)
    {
        var message = evt switch
        {
            AgentActivatedEvent activated => $"{activated.Role} is working...",
            AgentCompletedEvent completed => $"{completed.Role} completed",
            BuildValidationEvent build => build.Success ? "✅ Build passed" : "❌ Build failed",
            _ => evt.GetType().Name
        };

        Activities.Add(new ActivityItem(evt.Timestamp, evt.GetType().Name, message));
    }
}
```

### SignalR Hub Example

For web applications using SignalR:

```csharp
[HubName("orchestration")]
public class OrchestrationHub : Hub
{
    public async Task RunOrchestrationAsync(string prompt)
    {
        var service = // Get from DI
        var cts = new CancellationTokenSource();

        var readEventsTask = Task.Run(async () =>
        {
            await foreach (var evt in service.Events.Reader.ReadAllAsync(cts.Token))
            {
                await Clients.Caller.SendAsync("OrchestrationEvent", new
                {
                    Type = evt.GetType().Name,
                    Message = FormatEventMessage(evt),
                    Timestamp = evt.Timestamp
                });
            }
        }, cts.Token);

        try
        {
            await service.RunAsync(
                new OrchestrationRequest(prompt),
                cts.Token
            );
        }
        finally
        {
            service.Events.Writer.TryComplete();
            await readEventsTask;
        }
    }

    private string FormatEventMessage(OrchestrationEvent evt) => evt switch
    {
        AgentActivatedEvent a => $"🤖 {a.Role} activated",
        BuildValidationEvent b => b.Success ? "✅ Build passed" : "❌ Build failed",
        ResearchCompletedEvent r => $"📚 Found {r.SourcesFound} sources",
        _ => evt.GetType().Name
    };
}
```

## Event Flow During Orchestration

Here's how events flow through a typical orchestration run:

```
1. OrchestrationStartedEvent
   ↓
2. AgentActivatedEvent (Planner)
3. AgentCompletedEvent (Planner)
   ↓
4. PhaseStartedEvent (Phase 1)
5. AgentActivatedEvent (Coder)
6. FileCreatedEvent (Program.cs)
7. FileCreatedEvent (Project.csproj)
8. AgentCompletedEvent (Coder)
9. PhaseCompletedEvent (Phase 1)
   ↓
10. BuildValidationEvent (Success: true)
   ↓
11. BuildReviewStartedEvent
12. AgentActivatedEvent (BuildReviewer)
13. AgentCompletedEvent (BuildReviewer)
14. BuildReviewCompletedEvent
   ↓
15. OrchestrationCompletedEvent (with summary and flow diagram)
```

### With Research & Failures

If research is needed or build fails:

```
BuildValidationEvent (Success: false)
   ↓
FixAttemptStartedEvent (Attempt 1)
   ├─ ResearchRequestedEvent (Coder → Researcher)
   ├─ AgentActivatedEvent (Researcher)
   ├─ ResearchCompletedEvent (sources back)
   ├─ AgentCommunicationEvent (Researcher → Coder)
   ├─ AgentActivatedEvent (Coder)
   ├─ AgentCompletedEvent (Coder)
   └─ FileCreatedEvent (corrected files)
   ↓
BuildValidationEvent (Success: true retry)
   ↓
FixAttemptCompletedEvent (Success: true)
```

## Advanced Event Processing

### Filtering Events by Type

```csharp
var buildEvents = service.Events.Reader.ReadAllAsync()
    .Where(evt => evt is BuildValidationEvent or FixAttemptStartedEvent);

await foreach (var evt in buildEvents)
{
    // Handle only build-related events
    Console.WriteLine($"Build event: {evt.GetType().Name}");
}
```

### Event Statistics & Metrics

```csharp
var eventStats = new Dictionary<string, int>();

await foreach (var evt in service.Events.Reader.ReadAllAsync())
{
    var eventType = evt.GetType().Name;
    if (!eventStats.ContainsKey(eventType))
        eventStats[eventType] = 0;
    eventStats[eventType]++;
}

foreach (var (type, count) in eventStats.OrderByDescending(x => x.Value))
{
    Console.WriteLine($"{type}: {count}");
}
```

### Building Activity Timeline

```csharp
var timeline = new List<(DateTimeOffset Time, string Event, string Agent)>();

await foreach (var evt in service.Events.Reader.ReadAllAsync())
{
    var agent = evt switch
    {
        AgentActivatedEvent a => a.Role.ToString(),
        AgentCompletedEvent a => a.Role.ToString(),
        _ => "System"
    };

    timeline.Add((evt.Timestamp, evt.GetType().Name, agent));
}

// Output timeline
foreach (var (time, eventName, agent) in timeline)
{
    Console.WriteLine($"{time:HH:mm:ss.fff} [{agent}] {eventName}");
}
```

## Performance Considerations

### Channel Capacity

The event channel has a bounded capacity of 1000 events with a "drop oldest" strategy:

```csharp
new BoundedChannelOptions(1000) { FullMode = BoundedChannelFullMode.DropOldest }
```

**Implications:**

- Fast producers + slow consumers: oldest events are discarded
- For UI dashboards: not critical (users see latest events anyway)
- For audit/logging: consider writing events to persistent storage before dropping

### Async Consumption

Always use `await foreach` for efficient event consumption:

```csharp
// ✅ Good — efficient
await foreach (var evt in service.Events.Reader.ReadAllAsync())
{
    await ProcessEventAsync(evt);
}

// ❌ Avoid — blocking
while (service.Events.Reader.TryRead(out var evt))
{
    ProcessEvent(evt);
}
```

### Cancellation

Always provide a `CancellationToken` to allow graceful shutdown:

```csharp
var cts = new CancellationTokenSource();

// Cancel after timeout
cts.CancelAfter(TimeSpan.FromMinutes(10));

try
{
    await foreach (var evt in service.Events.Reader.ReadAllAsync(cts.Token))
    {
        // Process
    }
}
catch (OperationCanceledException)
{
    // Handle cancellation
}
```

## Best Practices

1. **Always consume events concurrently** — start event consumption in background while running orchestration
2. **Use pattern matching** — cleaner event type checking with `evt is Type`
3. **Include timestamps** — all events have `Timestamp` for ordering and debugging
4. **Handle cancellation** — provide cancellation tokens for graceful shutdown
5. **Test event flows** — ConsoleFlowTraces sample demonstrates comprehensive event tracing
6. **Monitor event types** — track which events are most frequent for performance tuning
7. **Store important events** — persist critical events (errors, completions) for audit trails
8. **Transform for UI** — convert events to user-friendly messages before display

## See Also

- [Architecture](architecture.md) — System design and pipeline overview
- [All Agents](agents.md) — Understanding agent interactions
- [Researcher Agent](RESEARCHER_AGENT.md) — Research and inter-agent communication
- [ConsoleFlowTraces Sample](../samples/ConsoleFlowTraces) — Complete event streaming example
