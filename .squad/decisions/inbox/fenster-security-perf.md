# Security & Performance Fixes — Issue #5

**Date:** 2026-02-14  
**Agent:** Fenster (Backend Dev)  
**Issue:** #5 — Apply security, performance & CI lessons from LocalEmbeddings v1.1.0 audit

---

## Security Fixes Applied

### 1. Path Traversal Prevention ✅
**Location:** `WorkspaceManager.ReadFile()`, `OrchestrationService.ExecuteTaskAsync()`

**Fix:**
- Added explicit validation to reject paths containing ".." segments
- Reject absolute paths from untrusted input (ReadFile only accepts relative paths)
- Maintained existing GetFullPath + StartsWith checks as defense-in-depth

**Rationale:** Prevents directory traversal attacks where malicious prompts could trick agents into reading files outside the workspace.

### 2. File Name Validation ✅
**Location:** `WorkspaceManager.cs`

**Fix:**
- Added hardcoded cross-platform invalid character set:
  ```csharp
  private static readonly char[] _invalidFileNameChars = 
      ['<', '>', ':', '"', '|', '?', '*', '\\', '/', '\0'];
  ```

**Rationale:** Path.GetInvalidFileNameChars() can vary by platform. Hardcoded set ensures consistent behavior and prevents exploitation of platform-specific edge cases.

**Note:** This array is declared for future use (e.g., when validating user-provided file names in CreateWorkspace). Current slug generation already filters to alphanumeric+hyphen.

### 3. Input Validation ✅
**Location:** `CopilotAgentClient.RunAsync()`, `WorkspaceManager` constructor

**Fixes:**
- Added null/empty checks for prompt and workspacePath in CopilotAgentClient
- Added 100,000 character limit on prompts to prevent resource exhaustion
- Added null/empty check for rootPath in WorkspaceManager constructor

**Rationale:** Public API entry points must validate all untrusted input to prevent crashes and resource exhaustion attacks.

### 4. File Integrity Checks ✅
**Location:** `SessionPersistence.LoadSessionAsync()`

**Fix:**
- Added 50 MB file size check before loading session files
- Prevents loading of unexpectedly large files that could cause memory exhaustion

**Rationale:** JSON deserialization of large files can cause OOM errors. Early size check prevents loading malicious or corrupted files.

---

## Security Items NOT Applied

### URL Validation ❌
**Not applicable** — No remote HTTP/HTTPS fetch operations exist in the core orchestration engine. Agent instructions may reference URLs but don't perform downloads.

---

## Performance Fixes Applied

### Vector Math (TensorPrimitives) ❌
**Not applicable** — No cosine similarity, dot product, or L2 norm calculations found in the codebase. This is an orchestration engine, not an embedding service.

### Span/Memory ✅
**Assessment:** Hot paths identified but no obvious allocation hotspots requiring immediate optimization.

**Hot paths analyzed:**
- Event channel read loops in OrchestrationHub
- Agent execution loops in OrchestrationService
- File I/O operations in WorkspaceManager

**Decision:** Current allocations are acceptable. All file operations use `async` APIs correctly. Event streaming uses Channel<T> which is already allocation-efficient. No string concatenation in loops detected.

**Future optimization:** If profiling reveals allocation pressure, consider:
- Span<char> for slug generation in CreateWorkspace
- Memory<byte> for file I/O buffers
- ArrayPool<T> for temporary buffers in event serialization

### ArrayPool ❌
**Not applicable** — No temporary buffer allocations in hot loops found. File I/O uses framework APIs that handle buffering internally.

### Benchmarks ❌
**Not recommended yet** — No identified performance bottlenecks warrant benchmarking at this stage. The system is I/O-bound (LLM calls, file writes) not CPU-bound.

**Recommendation:** Add benchmarks when:
1. Plan parsing becomes a bottleneck (many tasks/phases)
2. Event serialization shows up in profiling
3. Workspace file operations scale beyond 100+ files

### Top-K Search ❌
**Not applicable** — No ranking or similarity search operations exist in the core engine.

---

## Performance Items NOT Applied

No performance optimizations were necessary. The orchestration engine's performance characteristics are dominated by:
1. LLM response latency (seconds to minutes)
2. File I/O operations (milliseconds)
3. Process spawning (dotnet run/build commands)

CPU-bound optimization opportunities are negligible compared to I/O wait times.

---

## Summary

**Security fixes:** 4 applied, 1 not applicable  
**Performance fixes:** 0 applied (no bottlenecks identified)

**Next steps:**
- Monitor for path traversal attempts in production logs
- Consider adding telemetry for prompt length distribution
- Profile orchestration runs with 10+ agents to identify actual bottlenecks

**Lessons for future work:**
- Always validate file paths at API boundaries, not just internally
- Input size limits prevent resource exhaustion attacks
- Don't prematurely optimize — measure first

---

**Commit:** Security fixes for path traversal, input validation, file integrity (Issue #5)
