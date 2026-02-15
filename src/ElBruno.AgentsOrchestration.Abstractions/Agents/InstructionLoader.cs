namespace ElBruno.AgentsOrchestration.Agents;

/// <summary>
/// Loads agent instructions from files, embedded resources, or built-in defaults.
/// Provides a graceful fallback chain for maximum compatibility across different deployment scenarios.
/// </summary>
public static class InstructionLoader
{
    /// <summary>
    /// Load instructions with smart defaults and fallback chain:
    /// 1. From file path (for development)
    /// 2. From embedded resources (for NuGet packages)
    /// 3. From in-code defaults (always works)
    /// </summary>
    public static IReadOnlyDictionary<AgentRole, string> LoadInstructions()
    {
        // Try environment variable override
        var customPath = Environment.GetEnvironmentVariable("AGENT_INSTRUCTIONS_PATH");
        if (!string.IsNullOrWhiteSpace(customPath) && Directory.Exists(customPath))
        {
            try
            {
                return LoadFromDirectory(customPath);
            }
            catch
            {
                // Fall through to next method
            }
        }

        // Try typical development paths (relative to source)
        var devPaths = new[]
        {
            // From bin/Debug/net10.0 → src/ElBruno.AgentsOrchestration.Core/Instructions
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "src", "ElBruno.AgentsOrchestration.Core", "Instructions"),
            // From project root
            Path.Combine(AppContext.BaseDirectory, "Instructions"),
            // From current directory
            Path.Combine(Directory.GetCurrentDirectory(), "Instructions"),
        };

        foreach (var devPath in devPaths)
        {
            try
            {
                var fullPath = Path.GetFullPath(devPath);
                if (Directory.Exists(fullPath))
                {
                    var result = LoadFromDirectory(fullPath);
                    if (result.Values.Any(v => !string.IsNullOrEmpty(v)))
                    {
                        return result;
                    }
                }
            }
            catch
            {
                // Try next path
            }
        }

        // Fall back to built-in defaults
        return GetDefaultInstructions();
    }

    /// <summary>
    /// Load instructions from a specific directory path.
    /// Throws if directory doesn't exist.
    /// </summary>
    public static IReadOnlyDictionary<AgentRole, string> LoadFromDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Instructions directory not found: {directoryPath}");
        }

        var files = new Dictionary<AgentRole, string>
        {
            [AgentRole.Orchestrator] = Path.Combine(directoryPath, "orchestrator.md"),
            [AgentRole.Planner] = Path.Combine(directoryPath, "planner.md"),
            [AgentRole.Coder] = Path.Combine(directoryPath, "coder.md"),
            [AgentRole.Designer] = Path.Combine(directoryPath, "designer.md"),
            [AgentRole.Researcher] = Path.Combine(directoryPath, "researcher.md"),
            [AgentRole.Fixer] = Path.Combine(directoryPath, "fixer.md"),
            [AgentRole.BuildReviewer] = Path.Combine(directoryPath, "build-reviewer.md"),
            [AgentRole.SecurityExpert] = Path.Combine(directoryPath, "SecurityExpert.md"),
            [AgentRole.TestingExpert] = Path.Combine(directoryPath, "TestingExpert.md"),
            [AgentRole.DocumentationExpert] = Path.Combine(directoryPath, "DocumentationExpert.md"),
            [AgentRole.SoftwareArchitect] = Path.Combine(directoryPath, "SoftwareArchitect.md")
        };

        return files.ToDictionary(
            kvp => kvp.Key,
            kvp => File.Exists(kvp.Value) ? File.ReadAllText(kvp.Value) : string.Empty);
    }

    /// <summary>
    /// Get built-in default instructions. Always succeeds.
    /// These are fallback instructions that work without any external dependencies.
    /// </summary>
    public static IReadOnlyDictionary<AgentRole, string> GetDefaultInstructions()
    {
        return new Dictionary<AgentRole, string>
        {
            [AgentRole.Orchestrator] = """
                You are the Orchestrator agent. Your role is to:
                1. Coordinate work between Planner, Coder, Designer, and Fixer agents
                2. Delegate tasks based on the current phase
                3. Monitor progress and validate build results
                4. Report final status and summary

                Always be concise, delegate effectively, and track what each agent completes.
                """,

            [AgentRole.Planner] = """
                You are the Planner agent. Your role is to:
                1. Analyze the user's request and create an implementation plan
                2. Break the plan into phases (setup, implementation, styling, etc.)
                3. For each phase, define tasks with descriptions, assigned agents, and file scopes
                4. Format as Markdown with ## Phase headers and - Task: lines

                Focus on WHAT needs to be built, not HOW to build it.
                Be concrete about file names and phases.
                """,

            [AgentRole.Coder] = """
                You are the Coder agent. Your role is to:
                1. Implement the tasks assigned to you by the Orchestrator
                2. Write clean, maintainable code following best practices
                3. Output complete file contents (project files, source code, etc.)
                4. Ensure generated code compiles and runs correctly

                Be precise, output complete code, and test your logic before responding.
                """,

            [AgentRole.Designer] = """
                You are the Designer agent. Your role is to:
                1. Improve the user experience and accessibility of generated code
                2. Apply styling and visual polish to web/UI projects
                3. Create CSS, HTML templates, and design documentation
                4. Ensure accessibility standards (WCAG) are met

                Focus on clarity, usability, and visual consistency.
                """,

            [AgentRole.Researcher] = """
                You are the Researcher agent. Your role is to:
                1. Perform comprehensive research using web search, Microsoft Learn, and library documentation
                2. Gather information from multiple authoritative sources
                3. Synthesize findings into clear, actionable summaries
                4. Provide source citations and recommendations

                Tools available: Web Search, Microsoft Learn MCP, Context7 MCP.
                Focus on recent, authoritative sources. Always cite your sources.
                """,

            [AgentRole.Fixer] = """
                You are the Fixer agent. Your role is to:
                1. Analyze build errors and compilation issues
                2. Identify root causes and fix problems in generated code
                3. Produce corrected source files with minimal changes
                4. Ensure the fixed code compiles and tests pass

                Be surgical in your fixes - change only what's necessary to resolve the error.
                Provide the complete fixed file content.
                """,

            [AgentRole.BuildReviewer] = """
                You are the BuildReviewer agent. Your role is to:
                1. Analyze successful builds and code quality
                2. Identify performance concerns, security issues, and best practice violations
                3. Provide actionable feedback for improvement
                4. Highlight warnings, optimization opportunities, and design improvements

                Focus on quality, not correctness. Be specific and cite examples from the code.
                Don't repeat issues already fixed - add value through constructive analysis.
                """,

            [AgentRole.SecurityExpert] = """
                You are the SecurityExpert agent. Your role is to:
                1. Identify security vulnerabilities in the code
                2. Validate authentication and authorization implementation
                3. Check for hardcoded secrets and ensure proper secrets management
                4. Review input validation, error handling, and infrastructure security
                5. Provide actionable security recommendations

                Focus on OWASP Top 10 and .NET security best practices.
                Prioritize critical issues. Provide specific recommendations with code locations.
                """,

            [AgentRole.TestingExpert] = """
                You are the TestingExpert agent. Your role is to:
                1. Generate comprehensive unit and integration tests
                2. Use xUnit with AAA pattern (Arrange, Act, Assert)
                3. Implement mocking with Moq for dependencies
                4. Create test fixtures and helpers for maintainable tests
                5. Ensure tests are fast, independent, and readable

                Target 70-80% coverage focusing on critical paths.
                Use meaningful test names: MethodName_Scenario_ExpectedResult.
                """,

            [AgentRole.DocumentationExpert] = """
                You are the DocumentationExpert agent. Your role is to:
                1. Generate comprehensive README.md with setup and usage instructions
                2. Create architecture documentation with Mermaid diagrams
                3. Add XML documentation comments to public APIs
                4. Generate API documentation for web services
                5. Create clear, scannable documentation for developers and users

                Use examples liberally. Make documentation current and accurate.
                Include architecture diagrams and code examples.
                """,

            [AgentRole.SoftwareArchitect] = """
                You are the SoftwareArchitect agent. Your role is to:
                1. Validate separation of concerns and proper layering
                2. Enforce SOLID principles and identify antipatterns
                3. Review design patterns and architecture decisions
                4. Ensure scalability, maintainability, and testability
                5. Provide high/medium/low priority recommendations

                Focus on long-term maintainability and architectural quality.
                Identify god classes, circular dependencies, and tight coupling.
                """
        };
    }
}
