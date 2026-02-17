using ElBruno.AgentsOrchestration.Agents;

namespace ElBruno.AgentsOrchestration.AgentRepository.Models;

/// <summary>
/// Represents a unique identifier for an agent that can be either a static role or a dynamic agent.
/// </summary>
public sealed class AgentIdentifier : IEquatable<AgentIdentifier>
{
    public AgentRole? StaticRole { get; }
    public string? DynamicName { get; }
    public bool IsDynamic => DynamicName != null;
    
    private AgentIdentifier(AgentRole? staticRole, string? dynamicName)
    {
        if (staticRole == null && string.IsNullOrWhiteSpace(dynamicName))
        {
            throw new ArgumentException("Either static role or dynamic name must be provided");
        }
        
        StaticRole = staticRole;
        DynamicName = dynamicName;
    }

    public static AgentIdentifier FromRole(AgentRole role) => new(role, null);
    
    public static AgentIdentifier FromDynamicName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new(null, name);
    }

    public string GetName() => DynamicName ?? StaticRole?.ToString() ?? "Unknown";

    public bool Equals(AgentIdentifier? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        if (IsDynamic && other.IsDynamic)
        {
            return string.Equals(DynamicName, other.DynamicName, StringComparison.OrdinalIgnoreCase);
        }
        
        return StaticRole == other.StaticRole;
    }

    public override bool Equals(object? obj) => obj is AgentIdentifier other && Equals(other);

    public override int GetHashCode()
    {
        return IsDynamic 
            ? StringComparer.OrdinalIgnoreCase.GetHashCode(DynamicName!) 
            : StaticRole!.Value.GetHashCode();
    }

    public override string ToString() => GetName();
}
