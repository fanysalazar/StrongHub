namespace StrongHub.Models;

/// <summary>Strategy: each badge is one rule. Adding a badge means adding a class, nothing else changes.</summary>
public interface IBadgeRule
{
    string Code { get; }
    string Name { get; }
    BadgeKind Kind { get; }

    /// <summary>Text shown to the member (and in the timeline) when the badge is given.</summary>
    string Message { get; }

    bool IsSatisfied(BadgeContext context);
}
