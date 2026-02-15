# Publishing a New Version to NuGet

This guide covers how to publish new versions of the ElBruno.AgentsOrchestration packages to NuGet.org using GitHub Actions and NuGet Trusted Publishing (keyless, OIDC-based).

## Packages

| Package | Source | Description |
|---------|--------|-------------|
| ElBruno.AgentsOrchestration.Abstractions | `src/ElBruno.AgentsOrchestration.Abstractions/` | Core abstractions — agent roles, configuration, and IAgentClient interface |
| ElBruno.AgentsOrchestration.Orchestration | `src/ElBruno.AgentsOrchestration.Orchestration/` | Orchestration engine — 6-step pipeline with event streaming |
| ElBruno.AgentsOrchestration.Core | `src/ElBruno.AgentsOrchestration.Core/` | Complete toolkit — TemplateAgentClient, WorkspaceManager, and agent instructions |

> **Maintenance rule**: If a new packable library is added under `src/`, update `.github/workflows/publish.yml` in the same PR so the new project is packed/pushed, and add a matching NuGet Trusted Publishing policy.

## Prerequisites (One-Time Setup)

These steps only need to be done once.

### 1. Configure NuGet.org Trusted Publishing Policies

1. Sign in to [nuget.org](https://www.nuget.org/)
2. Click your username → Trusted Publishing
3. Add a policy for each package with these values:

| Field | Value |
|-------|-------|
| Repository Owner | `elbruno` |
| Repository | `elbruno.agentsorchestration` |
| Workflow File | `publish.yml` |
| Environment | `release` |

You need to create this policy three times — once per package:

- `ElBruno.AgentsOrchestration.Abstractions`
- `ElBruno.AgentsOrchestration.Orchestration`
- `ElBruno.AgentsOrchestration.Core`

> **Note**: For new packages that don't exist on NuGet.org yet, you must first push them once (the workflow handles this). After the initial push, add the Trusted Publishing policy so future publishes are keyless.

### 2. Configure GitHub Repository

1. Go to the repo Settings → Environments
2. Create an environment called `release`
   - Optionally add required reviewers if you want a manual approval gate before publishing
3. Go to Settings → Secrets and variables → Actions
4. Add a repository secret:
   - Name: `NUGET_USER`
   - Value: `elbruno` (your NuGet.org profile name — not your email)

## Publishing a New Version

### Option A: Create a GitHub Release (Recommended)

This is the standard workflow — the version is derived from the release tag.

1. **Update the version** in all three csproj files:
   - `src/ElBruno.AgentsOrchestration.Abstractions/ElBruno.AgentsOrchestration.Abstractions.csproj`
   - `src/ElBruno.AgentsOrchestration.Orchestration/ElBruno.AgentsOrchestration.Orchestration.csproj`
   - `src/ElBruno.AgentsOrchestration.Core/ElBruno.AgentsOrchestration.Core.csproj`

   ```xml
   <Version>1.0.0</Version>
   ```

2. **NuGet icon source** (already configured):
   - `images/logo_01.png`
   - Packed into each `.nupkg` as `logo_01.png` via `<PackageIcon>logo_01.png</PackageIcon>`

3. **Commit and push** the version change to `main`

4. **Create a GitHub Release**:
   - Go to the repo → Releases → Draft a new release
   - Create a new tag: `v1.0.0` (must match the version in the csproj)
   - Fill in the release title and notes
   - Click Publish release

5. **The Publish to NuGet workflow runs automatically**:
   - Strips the `v` prefix from the tag → uses `1.0.0` as the package version
   - Builds, tests, packs, and pushes to NuGet.org

### Option B: Manual Dispatch

Use this as a fallback or for testing.

1. Go to the repo → Actions → Publish to NuGet
2. Click Run workflow
3. Optionally enter a version (if left empty, the version from the csproj is used)
4. Click Run workflow

## How It Works

The workflow (`.github/workflows/publish.yml`) uses NuGet Trusted Publishing — no long-lived API keys are needed.

```
GitHub Release created (e.g. v1.0.0)
  → GitHub Actions triggers publish.yml
    → Builds + tests all projects
    → Packs three .nupkg files (Abstractions, Orchestration, Core)
    → Requests an OIDC token from GitHub
    → Exchanges the token with NuGet.org for a temporary API key (valid 1 hour)
    → Pushes all packages to NuGet.org
    → Temp key expires automatically
```

### Version Resolution Priority

The workflow determines the package version in this order:

1. **Release tag** — if triggered by a GitHub Release (strips leading `v`)
2. **Manual input** — if triggered via workflow dispatch with a version specified
3. **csproj fallback** — reads `<Version>` from `src/ElBruno.AgentsOrchestration.Core/ElBruno.AgentsOrchestration.Core.csproj`

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Workflow fails at "NuGet login" | Verify the Trusted Publishing policy on nuget.org matches the repo owner, repo name, workflow file, and environment exactly |
| NUGET_USER secret not found | Add the secret in GitHub repo Settings → Secrets → Actions |
| Package already exists | The `--skip-duplicate` flag prevents failures when re-pushing an existing version. Bump the version number instead |
| OIDC token errors | Ensure `id-token: write` permission is set in the workflow job |

## Reference Links

- [NuGet Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing) — Official docs on keyless OIDC-based publishing
- [NuGet/login GitHub Action](https://github.com/NuGet/login) — The action that exchanges OIDC tokens for temporary NuGet API keys
- [OpenID Connect (OIDC) in GitHub Actions](https://docs.github.com/en/actions/security-for-github-actions/security-hardening-your-deployments/about-security-hardening-with-openid-connect) — How GitHub Actions OIDC tokens work
- [GitHub Actions: Creating and Using Environments](https://docs.github.com/en/actions/managing-workflow-runs-and-deployments/managing-deployments/managing-environments-for-deployment) — How to configure the `release` environment with approval gates
- [NuGet Package Versioning](https://learn.microsoft.com/en-us/nuget/concepts/package-versioning) — Best practices for SemVer versioning
- [ElBruno.AgentsOrchestration on NuGet.org](https://www.nuget.org/packages/ElBruno.AgentsOrchestration.Core) — Published package page
