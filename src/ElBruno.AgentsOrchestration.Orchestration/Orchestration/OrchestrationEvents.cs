using ElBruno.AgentsOrchestration.Agents;

namespace ElBruno.AgentsOrchestration.Orchestration;

public abstract record OrchestrationEvent(DateTimeOffset Timestamp);

public sealed record OrchestrationStartedEvent(DateTimeOffset Timestamp, string Prompt) : OrchestrationEvent(Timestamp);
public sealed record PlanGeneratedEvent(DateTimeOffset Timestamp, string PlanMarkdown, ExecutionPlan Plan) : OrchestrationEvent(Timestamp);
public sealed record PlanApprovedEvent(DateTimeOffset Timestamp, bool AutoApproved) : OrchestrationEvent(Timestamp);
public sealed record PhaseStartedEvent(DateTimeOffset Timestamp, int PhaseIndex, string PhaseName) : OrchestrationEvent(Timestamp);
public sealed record AgentActivatedEvent(DateTimeOffset Timestamp, AgentRole Role, string TaskDescription, string InstructionPreview) : OrchestrationEvent(Timestamp);
public sealed record AgentStreamingEvent(DateTimeOffset Timestamp, AgentRole Role, string DeltaContent) : OrchestrationEvent(Timestamp);
public sealed record AgentInstructionUpdateEvent(DateTimeOffset Timestamp, AgentRole Role, string CurrentInstruction) : OrchestrationEvent(Timestamp);
public sealed record AgentCompletedEvent(DateTimeOffset Timestamp, AgentRole Role, string Result) : OrchestrationEvent(Timestamp);
public sealed record PhaseCompletedEvent(DateTimeOffset Timestamp, int PhaseIndex) : OrchestrationEvent(Timestamp);
public sealed record FileCreatedEvent(DateTimeOffset Timestamp, string FilePath) : OrchestrationEvent(Timestamp);
public sealed record BuildValidationEvent(DateTimeOffset Timestamp, bool Success, string Output) : OrchestrationEvent(Timestamp);
public sealed record FixAttemptStartedEvent(DateTimeOffset Timestamp, int Attempt, string BuildOutput) : OrchestrationEvent(Timestamp);
public sealed record FixAttemptCompletedEvent(DateTimeOffset Timestamp, int Attempt, bool Success, string Output) : OrchestrationEvent(Timestamp);
public sealed record BuildReviewStartedEvent(DateTimeOffset Timestamp, string BuildOutput) : OrchestrationEvent(Timestamp);
public sealed record BuildReviewCompletedEvent(DateTimeOffset Timestamp, string ReviewFeedback) : OrchestrationEvent(Timestamp);
public sealed record AppLaunchedEvent(DateTimeOffset Timestamp, string WorkspacePath) : OrchestrationEvent(Timestamp);
public sealed record AppStoppedEvent(DateTimeOffset Timestamp) : OrchestrationEvent(Timestamp);
public sealed record AppLogEvent(DateTimeOffset Timestamp, string LogLine) : OrchestrationEvent(Timestamp);
public sealed record OrchestrationCompletedEvent(
    DateTimeOffset Timestamp,
    OrchestrationResult FinalResult,
    string? AgentFlowDiagram = null,
    string? AgentFlowJson = null,
    AgentCallGraph? CallGraph = null,
    int TotalAgentCalls = 0,
    int LoopIterations = 0
) : OrchestrationEvent(Timestamp);
public sealed record OrchestrationErrorEvent(DateTimeOffset Timestamp, string Error) : OrchestrationEvent(Timestamp);

// ============================================================================
// Research and Inter-Agent Communication Events
// ============================================================================

public sealed record ResearchRequestedEvent(
    DateTimeOffset Timestamp,
    AgentRole RequestingAgent,
    string Query,
    ResearchScope Scope
) : OrchestrationEvent(Timestamp);

public sealed record ResearchCompletedEvent(
    DateTimeOffset Timestamp,
    AgentRole RequestingAgent,
    int SourcesFound,
    TimeSpan Duration
) : OrchestrationEvent(Timestamp);

public sealed record AgentCommunicationEvent(
    DateTimeOffset Timestamp,
    AgentRole FromAgent,
    AgentRole ToAgent,
    string CommunicationType, // "research_request", "research_response", "handoff", "retry"
    string Summary
) : OrchestrationEvent(Timestamp);
