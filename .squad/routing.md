# Squad Routing — AgentsOrchestration

## Routing Rules

| Work Type | Primary | Secondary | Notes |
|-----------|---------|-----------|-------|
| Architecture, design decisions, code review | Keaton | — | Lead makes judgment calls |
| .NET backend, orchestration, APIs, abstractions | Fenster | Keaton | Core library work |
| Blazor UI, dashboard, web components | Dallas | Keaton | Frontend & UX |
| Tests, xUnit, CI/Linux issues, quality | Hockney | Keaton | Test strategy & implementation |
| Backlog, work queue, PRs, follow-ups | Ralph | Keaton | Keep pipeline moving |
| Docs, changelogs, session logs | Scribe | — | Append-only files |

## Issue Triage

**Squad labels:**
- `squad` — tagged for squad work
- `squad:{member}` — assigned to specific member (lead triages)

**Severity mapping:**
- `security` → Keaton (review) + Fenster (implementation)
- `performance` → Fenster (impl) + Hockney (benchmarks)
- `bug` → Keaton (triage) + relevant dev
- `enhancement` → Lead (scope) + relevant dev

