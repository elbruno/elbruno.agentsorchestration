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
}
