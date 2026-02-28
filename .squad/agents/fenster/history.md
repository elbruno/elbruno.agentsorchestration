# Fenster — Project History

**Role:** Backend Dev  
**Project:** AgentsOrchestration — .NET 10 multi-agent orchestration library  
**Stack:** C#, .NET 10, async/await, Blazor, ASP.NET Core  

## Project Summary

Multi-agent orchestration system implementing Burke Holland's "Ultralight Orchestration" pattern.

**Core Abstractions:**
- `IAgentClient` — interface for LLM calls (one method: RunAsync)
- `AgentRole` enum — 11 specialized agent roles
- `AgentConfiguration` — sealed record with role, instructions, model, temperature, limits
- `OrchestrationEvent` — sealed record hierarchy for event streaming

**Orchestration Engine:**
- `OrchestrationService` — 6-step pipeline coordination
- `AgentFactory` — instantiates agents
- `AgentConfigurationStore` — ConcurrentDictionary of configs (thread-safe, singleton)
- `WorkspaceManager` — timestamped workspace directories, file isolation

**Event System:**
- `Channel<OrchestrationEvent>` — unbounded channel for event streaming
- Event types: PlanCreated, ExecutionStarted, ExecutionCompleted, etc.
- Hub consumes via ReadAllAsync()

**Key Files:**
- `src/ElBruno.AgentsOrchestration.Core/Orchestration/OrchestrationService.cs`
- `src/ElBruno.AgentsOrchestration.Core/Agents/AgentFactory.cs`
- `src/ElBruno.AgentsOrchestration.Core/Orchestration/Models.cs`
- `src/ElBruno.AgentsOrchestration.Core/Instructions/`

## Learnings

### Issue #5: Security & Performance Audit (2026-02-14)

**Security Patterns Identified:**
1. **Path traversal prevention** — Critical for any file-based workspace system. Applied defense-in-depth: explicit ".." rejection + absolute path rejection + GetFullPath normalization + StartsWith boundary check. Implemented in WorkspaceManager.ReadFile() and OrchestrationService.ExecuteTaskAsync().

2. **Cross-platform file name validation** — Hardcoded invalid character set prevents platform-specific edge cases. Path.GetInvalidFileNameChars() varies by OS; explicit array ensures consistent behavior across Windows/Linux/macOS.

3. **Input validation at boundaries** — All public API entry points (IAgentClient.RunAsync, WorkspaceManager ctor) must validate untrusted input. Added null checks, size limits (100K chars for prompts, 50 MB for session files) to prevent resource exhaustion attacks.

4. **File integrity checks** — Always validate size before loading binary/JSON files. Prevents OOM errors from malicious or corrupted large files.

**Performance Optimization Opportunities:**
- **Current bottlenecks:** LLM latency (seconds), file I/O (milliseconds), process spawning (dotnet commands). CPU optimization is not the priority.
- **Span/Memory:** Hot paths identified (event loops, agent execution, file I/O) but no obvious allocation hotspots. Current async APIs are allocation-efficient.
- **ArrayPool:** No temporary buffer allocations in loops found. Framework handles buffering internally.
- **Benchmarks:** Not warranted yet. Add when plan parsing (many tasks), event serialization, or workspace file operations (100+ files) become bottlenecks.

**Architectural Notes:**
- Orchestration engine is I/O-bound, not CPU-bound. Performance optimizations should focus on reducing LLM round-trips, parallel agent execution, and workspace cleanup strategies, not micro-optimizations.
- Security fixes are defensive layers: malicious prompts could trick agents into generating paths like "../../etc/passwd". Multi-layer validation (explicit checks + normalization + boundary checks) prevents exploitation even if one layer fails.
- File size limits (50 MB sessions, 100K prompts) are reasonable for normal use but prevent abuse. Adjust if legitimate use cases exceed limits.

**Lessons for Future Work:**
- Always validate file paths at API boundaries (public methods), not just internal methods
- Input size limits are security controls, not just performance optimizations
- Don't prematurely optimize — measure first (profiling reveals actual hotspots)
- Defense-in-depth: multiple validation layers catch edge cases and platform differences
