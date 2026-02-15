var builder = DistributedApplication.CreateBuilder(args);

// ──────────────────────────────────────
// Core Services
// ──────────────────────────────────────

var api = builder.AddProject<Projects.AgentsOrchestration_Api>("api")
    .WithExternalHttpEndpoints();

var dashboard = builder.AddProject<Projects.AgentsOrchestration_Web>("dashboard")
    .WithReference(api)
    .WaitFor(api)
    .WithExternalHttpEndpoints();

// ──────────────────────────────────────

builder.Build().Run();
