# Available Agents

The ElBruno.AgentsOrchestration system includes **11 specialized AI agents** that coordinate to analyze requirements, generate code, validate quality, and provide recommendations. Each agent has specific responsibilities and expertise.

## Core Orchestration Agents (6-Step Pipeline)

These agents drive the main orchestration pipeline:

### 🗺️ Planner

**Responsibilities:**

- Analyzes the user prompt and requirements
- Creates a structured implementation plan
- Breaks down the work into logical phases and tasks
- Identifies dependencies and execution order
- Considers constraints and best practices

**Used in:** Step 1 (Plan)

### 🧭 Orchestrator

**Responsibilities:**

- Parses the Planner's plan
- Delegates tasks to appropriate specialized agents (Coder, Designer, etc.)
- Manages inter-agent communication and task distribution
- Validates intermediate results
- Coordinates the overall orchestration flow
- Provides final summary and report

**Used in:** Steps 2, 4, 6 (Parse, Verify, Report)

### 💻 Coder

**Responsibilities:**

- Generates application code based on the plan
- Implements business logic and core functionality
- Creates project structure and configuration
- Writes production-ready, maintainable code
- Follows C# and .NET best practices
- Integrates with other generated components

**Used in:** Step 3 (Execute)

### 🎨 Designer

**Responsibilities:**

- Creates user interface designs and layouts
- Generates UI code (Blazor, WPF, Console UIs, etc.)
- Implements responsive design principles
- Ensures consistent styling and user experience
- Creates visual components and templates

**Used in:** Step 3 (Execute)

### 🔧 Fixer

**Responsibilities:**

- Analyzes build errors and failures
- Diagnoses root causes of compilation errors
- Generates corrected code to fix issues
- Retries compilation after fixes
- Implements iterative error resolution
- Ensures the application builds successfully

**Used in:** Step 4 (Verify) — on-demand when build fails

### 📊 BuildReviewer

**Responsibilities:**

- Analyzes successful builds for quality metrics
- Evaluates code quality, performance, and best practices
- Generates actionable feedback and recommendations
- Assesses maintainability and scalability
- Provides security analysis
- Suggests improvements for future iterations

**Used in:** Step 5 (Review)

## Specialist Agents (Extended Capabilities)

These agents provide specialized expertise and can be consulted for deeper analysis:

### 🔎 Researcher

**Responsibilities:**

- Performs web search and documentation lookups
- Gathers external knowledge (Microsoft Learn, Context7 API docs, etc.)
- Enriches agent context with research findings
- Provides citations and references
- Supports complex implementation decisions
- Bridges knowledge gaps during orchestration

**Capabilities:**

- Web search integration
- Microsoft Learn documentation access
- Context7 documentation search
- Real-time external knowledge retrieval

**Used in:** Inter-agent communication — any agent can request research

### 🔒 SecurityExpert

**Responsibilities:**

- Identifies security vulnerabilities
- Validates authentication and authorization patterns
- Enforces secure coding practices
- Checks for OWASP Top 10 vulnerabilities
- Reviews cryptography and data protection
- Validates secrets management

**Security Areas:**

- SQL injection, XSS, CSRF prevention
- JWT token validation
- OAuth2/OpenID Connect
- Cookie security
- Password hashing and storage
- Insecure deserialization
- Path traversal vulnerabilities
- Command injection risks

**Consulted by:** BuildReviewer, Coder, Fixer

### ✅ TestingExpert

**Responsibilities:**

- Generates unit tests with xUnit/NUnit
- Creates integration test suites
- Designs test strategy and coverage planning
- Implements mocking and test isolation
- Tests error handling and edge cases
- Ensures quality through comprehensive testing

**Test Types:**

- Unit tests (AAA pattern)
- Integration tests
- API endpoint testing
- Database testing
- Mock/stub generation
- Edge case and boundary testing

**Consulted by:** BuildReviewer, Coder

### 📚 DocumentationExpert

**Responsibilities:**

- Generates comprehensive README files
- Creates API documentation
- Builds architecture diagrams (Mermaid)
- Documents configuration and setup
- Writes inline code comments
- Creates user guides and tutorials
- Maintains contributing guidelines

**Documentation Types:**

- README.md with features and usage
- Architecture and design documentation
- API reference documentation
- Setup and installation guides
- Configuration documentation
- Code comments and docstrings

**Consulted by:** BuildReviewer, Orchestrator

### 🏗️ SoftwareArchitect

**Responsibilities:**

- Validates architectural decisions
- Enforces SOLID principles (S, O, L, I, D)
- Ensures separation of concerns
- Identifies architecture antipatterns
- Recommends design patterns
- Validates long-term maintainability and scalability

**Architecture Validation:**

- Layering (Presentation → Business → Data)
- Dependency Inversion
- Interface Segregation
- Design patterns (Dependency Injection, Factory, Observer, etc.)
- Repository pattern, Unit of Work
- Caching strategies
- Scalability considerations

**Consulted by:** BuildReviewer, Orchestrator, Coder

## Agent Communication Architecture

```
                    Orchestrator (🧭)
                    /    |    \    \
                   /     |     \    \
         Planner (🗺️)  Execute  Verify  Report
              |       /    \      |      |
              |      /      \     |      |
         Coder(💻) Designer(🎨) Fixer(🔧) BuildReviewer(📊)
              |                    |           /  |  \
              |                    |          /   |   \
         Research ──→ Researcher(🔎)   SecurityExpert(🔒)
         needed                        TestingExpert(✅)
                                      DocExpert(📚)
                                      Architect(🏗️)
```

## Agent Selection Guide

Choose the right agents based on your needs:

| Scenario | Primary Agents | Specialist Agents |
|----------|---|---|
| **Building web applications** | Coder, Designer, Orchestrator | SecurityExpert, TestingExpert, Architect |
| **API development** | Coder, Fixer | SecurityExpert, TestingExpert, DocExpert |
| **Desktop apps** | Coder, Designer, Fixer | TestingExpert, Architect |
| **Complex architecture** | Planner, Architect, Coder | Researcher, SoftwareArchitect |
| **Security-critical apps** | Coder, SecurityExpert, Fixer | TestingExpert, Architect |
| **Full documentation** | DocExpert, Coder | Researcher |

## Configuration Per Agent

Each agent can be configured with:

- **Model/LLM backend** — which AI model to use
- **Instructions** — custom system prompt (loaded from Markdown)
- **Tools** — web search, MCP servers, documentation access
- **Color/Icon** — for UI visualization
- **Temperature/creativity** — model-specific parameters

See [Library Packages](library-packages.md) for integration details and [Using the Libraries](using-the-libraries.md) for custom configuration examples.
