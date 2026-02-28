# Keaton — Project History

**Role:** Lead, Architecture  
**Project:** AgentsOrchestration — .NET 10 multi-agent orchestration library  
**Stack:** C#, .NET 10, Blazor, ASP.NET Core, xUnit  

## Project Summary

Multi-agent orchestration system with:
- 3 core library packages (Abstractions, Orchestration, Core)
- 6-step pipeline (Plan → Parse → Execute → Verify → Review → Report)
- 11 specialized agents (Orchestrator + 5 core + 5 specialists)
- Event streaming via Channel<OrchestrationEvent>
- Production Aspire app with Blazor dashboard, REST API, health checks, distributed tracing
- 40+ xUnit integration/unit tests

Key patterns:
- IAgentClient abstraction for LLM calls (currently TemplateAgentClient)
- Immutable sealed records for domain models
- Event stream for real-time updates
- Service lifetimes (singleton: AgentConfigurationStore, IAgentClient; scoped: WorkspaceManager, OrchestrationService)
- Workspace isolation with timestamped directories
- Agent instructions loaded from `.md` files with smart fallback chain
- Bootstrap CSS framework for web UI

## Learnings

### Issue #5: Security, Performance & CI Audit (2025-02-14)

**Context:** Applied lessons from LocalEmbeddings v1.1.0 audit (path traversal, cross-platform file names, input validation, CI hardening) to AgentsOrchestration codebase.

**Key Observations:**

1. **Strong Security Posture** — The codebase already implements robust security patterns:
   - Path traversal protection in `WorkspaceManager.ReadFile()` with `Path.GetFullPath()` normalization and `StartsWith()` validation
   - Comprehensive input validation using C# 11 `ArgumentException.ThrowIfNullOrWhiteSpace()` (23 usages across all public APIs)
   - Safe file name slug generation in workspace creation
   - All discovered with 100% test coverage in `SecurityAndEdgeCaseTests.cs`

2. **Performance Patterns Don't Apply** — Critical insight: LocalEmbeddings is compute-heavy (vector embeddings, cosine similarity), while AgentsOrchestration is I/O-bound (LLM calls, event streaming, file operations). Performance optimization patterns (TensorPrimitives, ArrayPool, Span<T>) are not applicable to orchestration workloads. The architecture is already efficient for its use case.

3. **Cross-Platform File Name Validation Bug** — Found one critical compatibility issue: `AwesomeCopilotAgentLoader.GetCacheFilePath()` uses `Path.GetInvalidFileNameChars()` which varies by platform (Windows allows more chars than Linux). This will cause CI failures on Linux. Must use hardcoded safe char set `[a-zA-Z0-9._-]` for predictable cross-platform behavior.

4. **CI Gap: No Automated Testing** — `squad-ci.yml` is a template stub ("No build commands configured"). PRs are not running `dotnet test` automatically. This is a high-priority risk — manual testing only.

5. **Publish Workflow Validation Gap** — `publish.yml` strips `v` prefix from git tags but doesn't validate tag format first. Malformed tags (e.g., `release-1.2.3` instead of `v1.2.3`) will produce incorrect package versions. Need regex validation before build.

**Architectural Implications:**

- **Security by Design Works** — The pattern of comprehensive input validation + path traversal checks at boundaries is effective. Maintain this pattern for all new public APIs.
- **Context-Aware Performance** — Don't blindly apply performance patterns from other projects. Orchestration frameworks need different optimizations (event streaming, async I/O, cancellation) than compute-heavy libraries (SIMD, memory pooling).
- **Cross-Platform Testing Critical** — Without Linux CI, cross-platform bugs (file names, path separators) go undetected. Configuring Squad CI is essential before next release.
- **Separation of Concerns in Testing** — `SecurityAndEdgeCaseTests.cs` demonstrates excellent test organization. Dedicated security test suite makes audit trivial and builds confidence in security posture.

**Decision:** Routed work to Fenster (backend cross-platform fixes) and Hockney (CI configuration). No architectural changes required — existing patterns are sound. Focus on CI hardening and cross-platform compatibility.
