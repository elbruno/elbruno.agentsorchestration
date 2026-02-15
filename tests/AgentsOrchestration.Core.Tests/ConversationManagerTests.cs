using ElBruno.AgentsOrchestration.Core.Orchestration;
using ElBruno.AgentsOrchestration.Orchestration;
using FluentAssertions;

namespace AgentsOrchestration.Core.Tests;

public sealed class ConversationManagerTests
{
    [Fact]
    public void CreateSession_GeneratesUniqueId()
    {
        var manager = new ConversationManager();
        var workspace1 = "/workspaces/session1";
        var workspace2 = "/workspaces/session2";

        var session1 = manager.CreateSession(workspace1);
        var session2 = manager.CreateSession(workspace2);

        session1.SessionId.Should().NotBeNull();
        session2.SessionId.Should().NotBeNull();
        session1.SessionId.Should().NotBe(session2.SessionId);
        session1.SessionId.Should().HaveLength(8); // GUID truncated to 8 chars
    }

    [Fact]
    public void CreateSession_ReturnsSessionWithEmptyHistory()
    {
        var manager = new ConversationManager();
        var workspace = "/workspaces/test";

        var session = manager.CreateSession(workspace);

        session.History.Should().BeEmpty();
        session.WorkspacePath.Should().Be(workspace);
        session.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RecordTurn_AddsToHistory()
    {
        var manager = new ConversationManager();
        var session = manager.CreateSession("/workspace");
        var prompt = "Create a console app";
        var result = new OrchestrationResult("Done", [], "/workspace");

        var updated = manager.RecordTurn(session.SessionId, prompt, result);

        updated.History.Should().HaveCount(1);
        updated.History[0].UserPrompt.Should().Be(prompt);
        updated.History[0].Result.Should().Be(result);
    }

    [Fact]
    public void RecordTurn_AppendedToExistingHistory()
    {
        var manager = new ConversationManager();
        var session = manager.CreateSession("/workspace");
        var result1 = new OrchestrationResult("Result 1", [], "/workspace");
        var result2 = new OrchestrationResult("Result 2", [], "/workspace");

        var s1 = manager.RecordTurn(session.SessionId, "First prompt", result1);
        var s2 = manager.RecordTurn(session.SessionId, "Second prompt", result2);

        s2.History.Should().HaveCount(2);
        s2.History[0].UserPrompt.Should().Be("First prompt");
        s2.History[1].UserPrompt.Should().Be("Second prompt");
    }

    [Fact]
    public void RecordTurn_ThrowsWhenSessionNotFound()
    {
        var manager = new ConversationManager();
        var result = new OrchestrationResult("Done", [], "/workspace");

        var action = () => manager.RecordTurn("nonexistent", "prompt", result);

        action.Should().Throw<KeyNotFoundException>()
            .WithMessage("*'nonexistent'*");
    }

    [Fact]
    public void GetSession_ReturnsSession()
    {
        var manager = new ConversationManager();
        var session = manager.CreateSession("/workspace");

        var retrieved = manager.GetSession(session.SessionId);

        retrieved.Should().NotBeNull();
        retrieved!.SessionId.Should().Be(session.SessionId);
    }

    [Fact]
    public void GetSession_ReturnsNullWhenNotFound()
    {
        var manager = new ConversationManager();

        var retrieved = manager.GetSession("nonexistent");

        retrieved.Should().BeNull();
    }

    [Fact]
    public void ListSessions_ReturnsAllSessions()
    {
        var manager = new ConversationManager();
        var s1 = manager.CreateSession("/workspace1");
        var s2 = manager.CreateSession("/workspace2");
        var s3 = manager.CreateSession("/workspace3");

        var list = manager.ListSessions();

        list.Should().HaveCount(3);
        list.Should().Contain(s => s.SessionId == s1.SessionId);
        list.Should().Contain(s => s.SessionId == s2.SessionId);
        list.Should().Contain(s => s.SessionId == s3.SessionId);
    }

    [Fact]
    public void ListSessions_ReturnsEmptyWhenNoSessions()
    {
        var manager = new ConversationManager();

        var list = manager.ListSessions();

        list.Should().BeEmpty();
    }

    [Fact]
    public void ClearSession_RemovesSession()
    {
        var manager = new ConversationManager();
        var session = manager.CreateSession("/workspace");

        manager.ListSessions().Should().HaveCount(1);

        manager.ClearSession(session.SessionId);

        manager.ListSessions().Should().BeEmpty();
        manager.GetSession(session.SessionId).Should().BeNull();
    }

    [Fact]
    public void ClearSession_DoesNotThrowIfNotFound()
    {
        var manager = new ConversationManager();

        // Should not throw
        var action = () => manager.ClearSession("nonexistent");

        action.Should().NotThrow();
    }

    [Fact]
    public void ConcurrentAccess_IsThreadSafe()
    {
        var manager = new ConversationManager();
        var sessions = new List<string>();
        var lockObj = new object();

        // Create 100 sessions concurrently
        var createTasks = Enumerable.Range(0, 100)
            .Select(i => Task.Run(() =>
            {
                var s = manager.CreateSession($"/workspace{i}");
                lock (lockObj)
                {
                    sessions.Add(s.SessionId);
                }
            }))
            .ToArray();

        Task.WaitAll(createTasks);

        // All should be unique
        sessions.Distinct().Should().HaveCount(100);
        manager.ListSessions().Should().HaveCount(100);
    }

    [Fact]
    public void ConversationSession_BuildContextPrompt_WithEmptyHistory()
    {
        var session = new ConversationSession(
            "sess-1", "/workspace", [], DateTimeOffset.UtcNow);

        var context = session.BuildContextPrompt("User request");

        context.Should().Be("User request");
    }

    [Fact]
    public void ConversationSession_BuildContextPrompt_WithHistory()
    {
        var turn1 = new ConversationTurn(
            "Create an app",
            new OrchestrationResult("App created", [], "/workspace"),
            DateTimeOffset.UtcNow);

        var history = (IReadOnlyList<ConversationTurn>)[turn1];
        var session = new ConversationSession("sess-1", "/workspace", history, DateTimeOffset.UtcNow);

        var context = session.BuildContextPrompt("Add a feature");

        context.Should().Contain("Conversation so far:");
        context.Should().Contain("[User]: Create an app");
        context.Should().Contain("[Assistant]: App created");
        context.Should().Contain("[User]: Add a feature");
    }

    [Fact]
    public void ConversationSession_AddTurn_AppendsAndReturnsNew()
    {
        var session = new ConversationSession(
            "sess-1", "/workspace", [], DateTimeOffset.UtcNow);

        var result = new OrchestrationResult("Done", [], "/workspace");
        var updated = session.AddTurn("Prompt 1", result);

        // Original should be unchanged (immutable)
        session.History.Should().BeEmpty();

        // New session should have the turn
        updated.History.Should().HaveCount(1);
        updated.History[0].UserPrompt.Should().Be("Prompt 1");
    }

    [Fact]
    public void ConversationSession_AddTurn_PreservesExistingHistory()
    {
        var turn1 = new ConversationTurn(
            "First prompt",
            new OrchestrationResult("First result", [], "/workspace"),
            DateTimeOffset.UtcNow);

        var history = (IReadOnlyList<ConversationTurn>)[turn1];
        var session = new ConversationSession("sess-1", "/workspace", history, DateTimeOffset.UtcNow);

        var result2 = new OrchestrationResult("Second result", [], "/workspace");
        var updated = session.AddTurn("Second prompt", result2);

        updated.History.Should().HaveCount(2);
        updated.History[0].UserPrompt.Should().Be("First prompt");
        updated.History[1].UserPrompt.Should().Be("Second prompt");
    }

    [Fact]
    public void ConversationSession_EstimateTokenCount_EmptyHistory()
    {
        var session = new ConversationSession("sess-1", "/workspace", [], DateTimeOffset.UtcNow);

        var tokenCount = session.EstimateTokenCount();

        tokenCount.Should().Be(0);
    }

    [Fact]
    public void ConversationSession_EstimateTokenCount_WithHistory()
    {
        // "Create app" = 10 chars, "App created" = 11 chars, total = 21 chars → ~5 tokens
        var turn1 = new ConversationTurn(
            "Create app",
            new OrchestrationResult("App created", [], "/workspace"),
            DateTimeOffset.UtcNow);

        var history = (IReadOnlyList<ConversationTurn>)[turn1];
        var session = new ConversationSession("sess-1", "/workspace", history, DateTimeOffset.UtcNow);

        var tokenCount = session.EstimateTokenCount();

        // 21 chars / 4 = ~5 tokens
        tokenCount.Should().Be(5);
    }

    [Fact]
    public void ConversationSession_TrimHistory_AlreadyWithinBudget()
    {
        var turn1 = new ConversationTurn(
            "Test",
            new OrchestrationResult("Result", [], "/workspace"),
            DateTimeOffset.UtcNow);

        var history = (IReadOnlyList<ConversationTurn>)[turn1];
        var session = new ConversationSession("sess-1", "/workspace", history, DateTimeOffset.UtcNow);

        var trimmed = session.TrimHistory(100);

        trimmed.History.Should().HaveCount(1);
        trimmed.Should().Be(session); // No change
    }

    [Fact]
    public void ConversationSession_TrimHistory_PreservesFirstTurn()
    {
        var turns = new List<ConversationTurn>
        {
            new("First prompt with many characters to increase token count",
                new OrchestrationResult("First long result with many characters", [], "/workspace"),
                DateTimeOffset.UtcNow),
            new("Second prompt",
                new OrchestrationResult("Second result", [], "/workspace"),
                DateTimeOffset.UtcNow),
            new("Third prompt",
                new OrchestrationResult("Third result", [], "/workspace"),
                DateTimeOffset.UtcNow)
        };

        var session = new ConversationSession("sess-1", "/workspace", turns, DateTimeOffset.UtcNow);

        // Very low budget should keep at least first turn
        var trimmed = session.TrimHistory(1);

        trimmed.History.Should().NotBeEmpty();
        trimmed.History[0].UserPrompt.Should().Be("First prompt with many characters to increase token count");
    }

    [Fact]
    public void ConversationSession_TrimHistory_RemovesOldestFirst()
    {
        var turns = new List<ConversationTurn>
        {
            new("First",
                new OrchestrationResult("Result 1", [], "/workspace"),
                DateTimeOffset.UtcNow),
            new("Second prompt that gets removed",
                new OrchestrationResult("Result 2", [], "/workspace"),
                DateTimeOffset.UtcNow),
            new("Third",
                new OrchestrationResult("Result 3", [], "/workspace"),
                DateTimeOffset.UtcNow)
        };

        var session = new ConversationSession("sess-1", "/workspace", turns, DateTimeOffset.UtcNow);

        // Mid-range budget: should keep first and last, drop middle
        var trimmed = session.TrimHistory(20);

        trimmed.History.Should().Contain(t => t.UserPrompt == "First");
        trimmed.History.Should().Contain(t => t.UserPrompt == "Third");
    }

    [Fact]
    public void ConversationSession_TrimHistory_ThrowsOnInvalidMaxTokens()
    {
        var session = new ConversationSession("sess-1", "/workspace", [], DateTimeOffset.UtcNow);

        var action = () => session.TrimHistory(0);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*Max tokens must be positive*");
    }

    [Fact]
    public void ConversationSession_TrimToLastNTurns_KeepsCorrectCount()
    {
        var turns = new List<ConversationTurn>
        {
            new("Turn 1", new OrchestrationResult("R1", [], "/workspace"), DateTimeOffset.UtcNow),
            new("Turn 2", new OrchestrationResult("R2", [], "/workspace"), DateTimeOffset.UtcNow),
            new("Turn 3", new OrchestrationResult("R3", [], "/workspace"), DateTimeOffset.UtcNow),
            new("Turn 4", new OrchestrationResult("R4", [], "/workspace"), DateTimeOffset.UtcNow),
            new("Turn 5", new OrchestrationResult("R5", [], "/workspace"), DateTimeOffset.UtcNow)
        };

        var session = new ConversationSession("sess-1", "/workspace", turns, DateTimeOffset.UtcNow);

        var trimmed = session.TrimToLastNTurns(3);

        // Should keep: Turn 1 (always), Turn 4, Turn 5
        trimmed.History.Should().HaveCount(3);
        trimmed.History[0].UserPrompt.Should().Be("Turn 1");
        trimmed.History[1].UserPrompt.Should().Be("Turn 4");
        trimmed.History[2].UserPrompt.Should().Be("Turn 5");
    }

    [Fact]
    public void ConversationSession_TrimToLastNTurns_AlreadyWithinLimit()
    {
        var turns = new List<ConversationTurn>
        {
            new("Turn 1", new OrchestrationResult("R1", [], "/workspace"), DateTimeOffset.UtcNow),
            new("Turn 2", new OrchestrationResult("R2", [], "/workspace"), DateTimeOffset.UtcNow)
        };

        var session = new ConversationSession("sess-1", "/workspace", turns, DateTimeOffset.UtcNow);

        var trimmed = session.TrimToLastNTurns(5);

        trimmed.History.Should().HaveCount(2);
        trimmed.Should().Be(session);
    }

    [Fact]
    public void ConversationSession_TrimToLastNTurns_ThrowsOnInvalidN()
    {
        var session = new ConversationSession("sess-1", "/workspace", [], DateTimeOffset.UtcNow);

        var action = () => session.TrimToLastNTurns(0);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*Must keep at least 1 turn*");
    }
}
