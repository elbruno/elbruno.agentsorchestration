# BenchmarkDotNet Benchmarks

Performance benchmarks for critical orchestration paths.

## Running Benchmarks

```bash
dotnet run -c Release --project tests/AgentsOrchestration.Benchmarks
```

## Benchmarked Operations

- **Agent Session Creation**: Measures instantiation overhead
- **Agent Session RunAsync**: Full async agent execution cycle
- **Configuration Store GetAll**: Retrieving all 11 agent configurations
- **Configuration Store Update**: Updating a single agent configuration
- **Workspace CreateWorkspace**: Workspace initialization with slug sanitization

## CI Integration

Results can be captured in CI pipelines to track performance regressions over time.
