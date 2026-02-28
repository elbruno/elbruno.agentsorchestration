# Dallas — Project History

**Role:** Frontend Dev  
**Project:** AgentsOrchestration — .NET 10 multi-agent orchestration library  
**Stack:** Blazor, ASP.NET Core, Bootstrap, C#  

## Project Summary

Multi-agent orchestration system with production Aspire app featuring:
- **Blazor Dashboard:** Real-time UI for orchestration state
- **Home.razor:** Main page consuming OrchestrationEvents via SignalR
- **Components:** Shared Blazor components in Web/Components/
- **API:** REST endpoints for orchestration control
- **Health Checks:** System health monitoring
- **Distributed Tracing:** Observability via Aspire

**Key Patterns:**
- SignalR hub at `/hubs/orchestration` for event streaming
- Events mapped to anonymous `{ Type, Message, Timestamp }` objects
- Blazor parameter binding for component reusability
- Bootstrap CSS framework (loaded from `wwwroot/lib/bootstrap/`)

**Key Files:**
- `samples/ElBruno.AgentsOrchestration.AspireApp/Web/Components/Pages/Home.razor`
- `samples/ElBruno.AgentsOrchestration.AspireApp/Web/Hubs/OrchestrationHub.cs`
- `samples/ElBruno.AgentsOrchestration.AspireApp/Web/Components/`
- `samples/ElBruno.AgentsOrchestration.AspireApp/Web/Models/DashboardModels.cs`

## Learnings

*To be updated as work progresses.*
