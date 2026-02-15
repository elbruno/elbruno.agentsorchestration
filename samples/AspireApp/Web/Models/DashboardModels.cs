namespace AgentsOrchestration.Web.Models;

public sealed class AgentNodeState
{
    public string Name { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-5.3-codex";
    public string Instruction { get; set; } = "Idle";
    public string Status { get; set; } = "idle";
    public string Color { get; set; } = "#6c757d";
    public string Icon { get; set; } = "\U0001f9ed";
}

public sealed record ActivityItem(DateTimeOffset Timestamp, string Type, string Message);

public sealed record ChatMessage(string Role, string Content, DateTimeOffset Timestamp);

public sealed class AgentConnection
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
