# Hockney — Project History

**Role:** Tester  
**Project:** AgentsOrchestration — .NET 10 multi-agent orchestration library  
**Stack:** xUnit, C#, .NET 10, Linux CI  

## Project Summary

Multi-agent orchestration system with 40+ xUnit tests covering:
- Integration tests for orchestration pipeline
- Unit tests for agent factory and configuration
- Event stream validation
- Workspace management
- Async/await patterns

**Test Patterns:**
- xUnit [Fact] and [Theory] attributes
- [SkippableFact]/[SkippableTheory] for platform-conditional tests (CRITICAL: must use Skippable* not Fact/Theory on Linux)
- Temp directory creation + finally block cleanup
- CI runs on both Windows and Linux

**CI Issues to Watch:**
- Platform-specific file handling (Path.GetInvalidFileNameChars() insufficient on Linux)
- Skip.IfNot(IsWindows()) requires [SkippableFact] or test fails on Linux instead of skipping
- Cross-platform file name validation needs hardcoded char set

**Key Files:**
- `tests/AgentsOrchestration.Core.Tests/`
- Build via: `dotnet test`

## Learnings

*To be updated as work progresses.*
