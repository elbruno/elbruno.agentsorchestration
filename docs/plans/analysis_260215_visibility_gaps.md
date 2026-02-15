# Analysis: Agent Visibility and Execution Gaps

**Date**: 2026-02-15  
**Author**: System Analysis  
**Status**: Identified Issues — Awaiting Implementation

---

## Executive Summary

User observed that when submitting a prompt explicitly requesting online research for weather services, the Researcher agent did not appear to execute. This investigation revealed **four systemic gaps** between the documented capabilities and actual implementation:

1. **Researcher Agent Never Invoked** — Documentation exists, events defined, but orchestration never calls it
2. **Plan Output Hidden from User** — Planner generates plan, but UI doesn't display it
3. **Agent Output Not Fully Accessible** — Activity feed shows truncated messages; no way to view full output
4. **BuildReviewer Execution Not in Plan** — Runs separately from plan, not visible in plan phases

---

## Issue 1: Researcher Agent Is Never Invoked

### Current State

**Documentation Says:**

- Researcher agent exists with full instructions ([Researcher.md](../RESEARCHER_AGENT.md))
- `ResearchRequestedEvent` and `ResearchCompletedEvent` are defined in `OrchestrationEvents.cs`
- UI displays research events in activity feed and agent graph (purple node #6610f2)
- Documentation describes detailed research workflow with web search, Microsoft Learn, Context7 MCP

**Reality:**

- `OrchestrationService.cs` **NEVER** creates a `AgentFactory.CreateSession(AgentRole.Researcher)`
- `ResearchRequestedEvent` and `ResearchCompletedEvent` are **NEVER** published
- The only reference to Researcher is in `TemplateAgentClient.BuildResearcherOutput()` which returns mock research markdown
- When TemplateAgentClient detects "search"/"research"/"online" in prompt, it includes a research phase in the plan:

  ```
  ## Phase 1: Research
  - Task: Research available options | Agent: Researcher | File: research.md
  ```

- However, `OrchestrationService.ExecuteTaskAsync()` treats Researcher the same as Coder — it calls the agent, writes output to `research.md`, but **does not emit research-specific events**

### Root Cause

The Researcher agent is **architecturally designed** but **not operationally integrated**. The orchestration service implements a simple task execution loop that doesn't distinguish between research tasks and code generation tasks.

### Pros/Cons of Current Architecture

#### ✅ Pros (Why It Works This Way)

- **Simplicity**: Single execution path for all agents
- **Consistency**: All agents follow same input→output→file pattern
- **Testability**: Easy to test with TemplateAgentClient
- **Extensibility**: Easy to add new agent roles without modifying orchestration

#### ❌ Cons (Why It's Problematic)

- **Discoverability**: Users don't know research is happening (no distinct events)
- **Observability**: Can't trace research operations separately from code generation
- **Tool Access**: Research-specific tools (MCP servers, web search) not triggered
- **Event Streaming**: Dashboards show "Coder active" when research is actually occurring
- **Misleading Documentation**: Comprehensive docs describe features that don't execute

---

## Issue 2: Plan Output Is Not Visible to Users

### Current State

**What Happens:**

1. `OrchestrationService.BuildPlanAsync()` creates Planner agent
2. Publishes `AgentActivatedEvent` with "Building execution plan"
3. Calls `planner.RunAsync(prompt, workspacePath)` — returns full plan as markdown
4. Publishes `AgentInstructionUpdateEvent` with **full plan text**
5. Publishes `AgentCompletedEvent` with plan text
6. Parses plan into `ExecutionPlan` object for internal use

**UI Currently Shows:**

- Activity feed displays: "Planner completed" (truncated message)
- Agent graph shows Planner node as "complete"
- **Plan contents are hidden** — user never sees the actual phases/tasks

**Where Plan Text Goes:**

- `AgentInstructionUpdateEvent` contains full plan, but `Home.razor.ApplyEvent()` doesn't handle this event type for display
- `AgentCompletedEvent` contains plan in `Result` field, but activity feed only shows "Planner completed"

### User Impact

**For Your Weather Prompt:**
The Planner generated something like:

```markdown
# Implementation Plan

Prompt: Create a .NET console app that displays current weather...

## Phase 1: Research
- Task: Research available options | Agent: Researcher | File: research.md

## Phase 2: Project Setup
- Task: Create project file | Agent: Coder | File: project.csproj

## Phase 3: Core Implementation
- Task: Implement main application logic | Agent: Coder | File: Program.cs
- Task: Create data models | Agent: Coder | File: Models.cs
```

**User Never Saw This Plan**. You only saw:

- Activity feed: "Planner active: Building execution plan"
- Activity feed: "Planner completed"
- Then: "Coder active: Research available options"

### Pros/Cons

#### ✅ Pros (Current Approach)

- **Less UI Clutter**: Activity feed stays concise
- **Focus on Results**: Emphasizes generated files, not intermediate plans
- **Performance**: No expensive plan rendering in UI

#### ❌ Cons (Problems)

- **No Visibility**: User doesn't know what agents will do before they do it
- **Can't Intervene**: User can't stop orchestration if plan is wrong
- **Debugging**: Can't see if plan parsing is correct
- **Trust**: User doesn't see planning step, reduces transparency
- **Expectation Mismatch**: User asked for research, but plan doesn't explicitly show research happening (because it's in a task named "Research available options" assigned to Coder, not Researcher)

---

## Issue 3: Agent Output Not Fully Accessible

### Current State

**Events Published:**

- `AgentStreamingEvent` — Contains full agent output (e.g., entire Program.cs code)
- `AgentCompletedEvent` — Contains full agent output

**Home.razor.ApplyEvent():**

```csharp
else if (evt is AgentActivatedEvent activated2)
{
    SetNodeStatus(activated2.Role.ToString(), "active", message);
    // message = truncated to 90 chars in SetNodeStatus
}
else if (evt is AgentCompletedEvent completed2)
{
    SetNodeStatus(completed2.Role.ToString(), "complete", message);
    // message = "Coder completed"
}
```

**Activity Feed:**
Shows truncated messages like:

- "Coder active: Implement main application logic for: Create a .NET console app..."
- "File updated: Program.cs"
- "Coder completed"

**Agent Graph:**

- Shows agent nodes with status colors
- Shows truncated "instruction" text (max 30 chars in `AgentGraph.razor.Truncate()`)
- **No click interaction** — nodes are static SVG elements

### User Impact

**What You Can't Do:**

- ❌ Click on Planner bubble to see the generated plan
- ❌ Click on Coder bubble to see the generated Program.cs code
- ❌ Click on Researcher bubble (if it ran) to see research findings
- ❌ Click on BuildReviewer bubble to see quality analysis
- ❌ Expand activity feed items to see full details

**What You Can Do:**

- ✅ See timestamped activity log with brief descriptions
- ✅ See which agents ran and in what order
- ✅ See file creation events
- ✅ Copy workspace location and open files manually
- ✅ Launch the built app from UI

### Pros/Cons

#### ✅ Pros (Simplicity)

- **Clean UI**: No popups or modal dialogs cluttering interface
- **Performance**: No large text rendering in real-time
- **Focus**: Emphasizes file outputs (which are the real artifacts)

#### ❌ Cons (Discoverability)

- **Hidden Context**: Can't see what agents actually said/did
- **Debugging**: Hard to diagnose why agent made certain decisions
- **Learning**: User can't learn from agent reasoning
- **Verification**: Can't verify agent did research before seeing "research" event

---

## Issue 4: BuildReviewer Execution Not Part of Plan

### Current State

**Planner Instructions Say:**

```markdown
Example validation phase:

## Phase N: Validation
- Task: Build and validate generated project | Agent: Orchestrator | File: build-output.log
```

**What Actually Happens:**

1. **Planner creates plan** (phases 1-3: Setup, Implementation, Styling)
2. **Plan DOES NOT include BuildReviewer** (no Phase 4: Review)
3. **OrchestrationService.RunAsync() executes plan** → all tasks complete
4. **Then calls VerifyAsync()** → runs `dotnet build`, Fixer if needed
5. **Then calls ReviewAsync()** → creates BuildReviewer agent **separately**
6. **ReviewAsync() only runs if build succeeds** → BuildReviewer analyzes quality

### Root Cause

**Dual System:**

- **Plan**: User-facing task list (what agents will do)
- **Orchestration**: Hardcoded pipeline (Plan → Execute Tasks → Verify → Review)

BuildReviewer is part of the hardcoded pipeline step "Review", **not part of the dynamic plan**.

### Pros/Cons

#### ✅ Pros (Hardcoded Review Step)

- **Reliability**: Review always happens (can't be skipped by bad plan)
- **Separation**: Build validation and quality review are orchestration concerns, not user concerns
- **Simplicity**: No need to parse complex plan syntax for build steps

#### ❌ Cons (Transparency)

- **Invisible to Plan**: User sees plan with 3 phases, but 5 things actually happen (Plan, Execute, Verify, Review, Report)
- **Misleading**: Planner instructions say to include validation phase, but template ignores this
- **Inconsistency**: Some agents (Researcher, BuildReviewer) run outside plan; others run inside plan
- **Can't Customize**: User can't say "skip code review for this project" because it's not in plan

---

## Recommendations

### Option A: Quick Fixes (Low Effort, High Impact)

#### 1. Display Plan in UI (1-2 hours)

**Where:** `Home.razor`

Add a new section to display the plan:

```razor
@if (!string.IsNullOrEmpty(CurrentPlan))
{
    <div class="plan-panel">
        <h3>Execution Plan</h3>
        <MarkdownView Content="@CurrentPlan" />
    </div>
}
```

Handle `AgentInstructionUpdateEvent` for Planner:

```csharp
else if (evt is AgentInstructionUpdateEvent instruction && instruction.Role == AgentRole.Planner)
{
    CurrentPlan = instruction.CurrentInstruction;
}
```

**Impact:** User sees the plan before execution starts.

#### 2. Make Agent Nodes Clickable (2-3 hours)

**Where:** `AgentGraph.razor`, `Home.razor`

Convert SVG `<g>` elements to interactive buttons:

```razor
<g class="agent-node-clickable" @onclick="@(() => OnNodeClick(node.Name))">
    <!-- node contents -->
</g>
```

Store agent outputs:

```csharp
private Dictionary<string, string> _agentOutputs = new();

else if (evt is AgentCompletedEvent completed)
{
    _agentOutputs[completed.Role.ToString()] = completed.Result;
}
```

Show modal/panel on click:

```razor
@if (SelectedAgent is not null)
{
    <AgentOutputPanel Agent="@SelectedAgent" Output="@_agentOutputs[SelectedAgent]" OnClose="@(() => SelectedAgent = null)" />
}
```

**Impact:** User can click Planner to see plan, click Coder to see code, etc.

#### 3. Emit ResearchRequestedEvent in OrchestrationService (30 minutes)

**Where:** `OrchestrationService.ExecuteTaskAsync()`

```csharp
private async Task<TaskResult> ExecuteTaskAsync(ExecutionTask task, string workspacePath, CancellationToken cancellationToken)
{
    // Emit research event if Researcher agent
    if (task.AssignedRole == AgentRole.Researcher)
    {
        await PublishAsync(new ResearchRequestedEvent(
            DateTimeOffset.UtcNow,
            AgentRole.Orchestrator, // requester
            task.Description,
            ResearchScope.All
        ), cancellationToken);
    }

    var session = _agentFactory.CreateSession(task.AssignedRole);
    // ... existing code ...

    // Emit research completed event
    if (task.AssignedRole == AgentRole.Researcher)
    {
        await PublishAsync(new ResearchCompletedEvent(
            DateTimeOffset.UtcNow,
            AgentRole.Orchestrator,
            SourcesFound: 3, // parse from output
            Duration: TimeSpan.Zero // calculate
        ), cancellationToken);
    }

    await PublishAsync(new AgentCompletedEvent(DateTimeOffset.UtcNow, task.AssignedRole, output), cancellationToken);
    return new TaskResult(task.AssignedRole, task.Description, output);
}
```

**Impact:** Research operations become visible in activity feed and agent graph.

---

### Option B: Comprehensive Redesign (Medium Effort, Maximum Impact)

#### 1. Make Plan First-Class in UI (1 day)

**New Components:**

- `PlanViewer.razor` — Collapsible plan display with phase/task tree
- `PhaseProgress.razor` — Progress bar showing current phase
- Plan appears **before** execution starts with "Approve / Cancel / Modify" buttons

**Flow:**

1. User submits prompt
2. Planner generates plan
3. **UI displays full plan** with option to proceed or cancel
4. User clicks "Execute Plan" → orchestration starts
5. Plan panel updates in real-time (current phase highlighted)

**Impact:** Full transparency, user control, clear expectations.

#### 2. Unified Agent Output Store (2-3 hours)

**New Service:** `AgentOutputStore` (singleton)

```csharp
public sealed class AgentOutputStore
{
    private readonly ConcurrentDictionary<(string SessionId, AgentRole Role, DateTimeOffset Timestamp), AgentOutput> _outputs = new();
    
    public void StoreOutput(string sessionId, AgentRole role, string output, DateTimeOffset timestamp);
    public IReadOnlyList<AgentOutput> GetSessionOutputs(string sessionId);
    public AgentOutput? GetLatestOutput(string sessionId, AgentRole role);
}
```

**Usage:**

- `OrchestrationService` stores all agent outputs
- UI queries store on agent node click
- Persists across page refreshes (in-memory for now)

**Impact:** Centralized, queryable agent output history.

#### 3. Add Plan Validation Phase (3-4 hours)

**New Event:** `BuildReviewStartedEvent`, `BuildReviewCompletedEvent` (already exist!)

**Change:**
Make BuildReviewer part of the plan. Update `TemplateAgentClient.BuildPlanOutput()`:

```csharp
sb.AppendLine($"## Phase {phaseIndex++}: Quality Review");
sb.AppendLine($"- Task: Review code quality and best practices | Agent: BuildReviewer | File: review.md");
```

**Impact:** Full plan transparency, consistent agent invocation.

---

### Option C: Incremental Improvements (Hybrid Approach — Recommended)

**Week 1: Quick Wins**

1. Display plan text in collapsible panel (Option A.1)
2. Add click handlers to agent nodes showing modal with full output (Option A.2)
3. Emit research events when Researcher agent runs (Option A.3)

**Week 2: Structured Plan Display**
4. Create `PlanViewer.razor` component with phase/task tree
5. Add real-time phase progress indicator
6. Syntax highlight plan markdown

**Week 3: Agent Output Management**
7. Implement `AgentOutputStore` for centralized storage
8. Add "View All Agent Outputs" button to UI
9. Export agent outputs to markdown report

**Week 4: Plan Validation Phase**
10. Update Planner to include BuildReviewer in plan
11. Make review step optional (flag in OrchestrationConfiguration)
12. Add "Skip Review" button for fast iterations

---

## Comparison: Current vs. Proposed

| Aspect | Current | Option A (Quick) | Option B (Full) | Option C (Hybrid) |
|--------|---------|------------------|-----------------|-------------------|
| **Plan Visibility** | Hidden | Text panel | Interactive tree | Collapsible tree |
| **Agent Output Access** | Manual file open | Click node modal | Full output browser | Click node modal → evolve to browser |
| **Research Events** | Missing | Emitted | Emitted | Emitted (Week 1) |
| **BuildReviewer in Plan** | No | No | Yes | Yes (Week 4) |
| **User Intervention** | Cancel only | Cancel only | Approve/Modify | Approve/Cancel (Week 2) |
| **Implementation Time** | — | 4-6 hours | 3-4 days | 4 weeks |
| **Risk** | — | Low | Medium | Low (gradual) |

---

## Technical Debt Assessment

### Current Technical Debt

**Severity: Medium**

- Documentation describes features that don't exist (Researcher workflow)
- Events defined but never published (ResearchRequestedEvent, ResearchCompletedEvent)
- Instructions contradict implementation (Planner says include validation, template doesn't)
- UI provides no way to inspect intermediate results

**Impact:**

- User confusion (expected research, didn't happen)
- Poor debuggability (can't see what agents did)
- Reduced trust (can't verify agent behavior)

### If Not Fixed

**Consequences:**

1. **Onboarding Friction**: New users read docs, expect features, get confused
2. **Support Load**: Users report "Researcher not working" (because it's invisible)
3. **Extensibility**: Adding new agents follows inconsistent patterns
4. **Testing**: Hard to write integration tests without observability

---

## User Story: What Should Have Happened

### Your Prompt
>
> Create a .NET console app that displays current weather for three cities: Toronto, Tokyo, and Madrid. **Search online for free services** that can provide this information...

### Expected Flow (Full Implementation)

1. **Orchestration Started**
   - 🎯 Orchestrator: "Starting orchestration"

2. **Planning Phase**
   - 📋 Planner active: "Building execution plan"
   - **[UI SHOWS PLAN]**:

     ```
     Phase 1: Research → Researcher: Find free weather APIs
     Phase 2: Setup → Coder: Create .csproj
     Phase 3: Implementation → Coder: Implement weather logic
     Phase 4: Quality Review → BuildReviewer: Analyze code
     ```

   - 📋 Planner completed

3. **Phase 1: Research**
   - 🔍 Researcher active: "Researching free weather APIs"
   - 🔍 Research requested: "free weather API services"
   - **[Web search executes]**
   - **[Microsoft Learn MCP queries]**
   - ✅ Research completed: 3 sources found in 2.1s
   - 📄 File created: research.md
   - 🔍 Researcher completed

4. **Phase 2: Project Setup**
   - 💻 Coder active: "Create project file"
   - 📄 File created: project.csproj
   - 💻 Coder completed

5. **Phase 3: Core Implementation**
   - 💻 Coder active: "Implement weather API calls using Open-Meteo"
   - 📄 File created: Program.cs
   - 📄 File created: Models.cs
   - 💻 Coder completed

6. **Build Validation**
   - ✅ Build succeeded

7. **Phase 4: Quality Review**
   - 🧠 BuildReviewer active: "Reviewing code quality"
   - 🧠 BuildReviewer completed: "Code quality: Excellent. Async patterns used correctly."

8. **Completion**
   - ✅ Orchestration completed

**User sees:**

- Full plan with 4 phases
- Research happening explicitly (not hidden as "Coder: Research available options")
- Research sources found (Open-Meteo, OpenWeatherMap, WeatherAPI)
- Code quality feedback from BuildReviewer
- Can click any agent node to see detailed output

### Actual Flow (Current Implementation)

1. **Orchestration Started**
2. 📋 Planner active: "Building execution plan"
3. 📋 Planner completed *(plan hidden)*
4. 💻 Coder active: "Research available options"  
   *(This is research, but looks like coding task)*
5. 📄 File created: research.md
6. 💻 Coder completed
7. 💻 Coder active: "Create project file"
8. 📄 File created: project.csproj
9. *(... rest of execution ...)*
10. ✅ Build succeeded
11. 🧠 BuildReviewer active *(not in plan, appears suddenly)*
12. ✅ Orchestration completed

**User sees:**

- No plan
- "Coder" doing research (confusing)
- BuildReviewer appears without context
- No way to inspect what each agent actually did

---

## Conclusion

The system has **strong architectural foundations** (events, agents, workspace isolation) but **weak observability**. The gaps are not bugs — they're **intentional simplifications** that trade transparency for simplicity.

**Recommendation:** Implement **Option C (Hybrid)** over 4 weeks:

- Week 1 quick wins provide immediate value
- Week 2-4 builds structured, long-term solutions
- Gradual rollout minimizes risk
- Aligns with roadmap for observability enhancements

**Next Steps:**

1. Review this analysis with team
2. Decide on implementation option (A, B, or C)
3. Create detailed implementation plan
4. Assign tasks and timeline

---

## Related Files

- [Researcher Agent Documentation](../RESEARCHER_AGENT.md)
- [Event Streaming Guide](../event-streaming.md)
- [Architecture Overview](../architecture.md)
- [Roadmap: Observability Enhancements](roadmap.md#observability-and-debugging)
