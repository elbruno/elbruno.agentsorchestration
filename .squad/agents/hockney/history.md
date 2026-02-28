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

### 2026-02-28: CI/Linux Test Infrastructure Audit (Issue #5)

**Platform-Conditional Test Patterns:**
- **CRITICAL:** Tests using `Skip.IfNot(IsWindows())` MUST use `[SkippableFact]` not `[Fact]`
- On Linux, `Skip.*` throws `SkipException` which `[Fact]` treats as a FAILURE, not a skip
- Solution: Use `Xunit.SkippableFact` package (added proactively to all test projects)
- Pattern: `[SkippableFact] public void Test() { Skip.IfNot(IsWindows()); ... }`

**Cross-Platform File Validation:**
- `Path.GetInvalidFileNameChars()` is platform-specific (2 chars on Linux, 30+ on Windows)
- Tests relying on this API can pass on Windows but fail on Linux
- Solution: Use hardcoded cross-platform character set: `['<', '>', ':', '"', '|', '?', '*', '\\', '/', '\0']`

**Workflow Version Format Validation:**
- Git tag typos (e.g., `v.1.2.3` instead of `v1.2.3`) cause silent build failures
- Added pre-build validation step to `publish.yml` that fails fast with clear error
- Strips both leading `'v'` AND stray `'.'` from tag names
- Validates semantic version format before wasting CI time

**Performance Benchmarks:**
- Created BenchmarkDotNet project for critical paths: agent instantiation, workspace operations
- 5 benchmarks covering configuration store, workspace creation, and async execution
- Can be integrated into CI to track performance regressions over time
- Run with: `dotnet run -c Release --project tests/AgentsOrchestration.Benchmarks`

**Test Infrastructure Health:**
- No platform-conditional test violations found (clean codebase)
- No `GetInvalidFileNameChars()` usage found (clean codebase)
- 116 tests total (2 pre-existing failures unrelated to CI/Linux infrastructure)
- All 3 test projects now include `Xunit.SkippableFact` for future platform-specific tests

**Key Gotcha:** xUnit's `[Fact]` attribute does NOT support mid-test skipping. Always use `[SkippableFact]` when calling `Skip.*` methods inside the test body. This is the #1 cause of Linux CI failures in cross-platform .NET test suites.

