namespace StrongHub.Models;

/// <summary>Achievement badges reward good results; shame badges encourage members who fall behind.</summary>
public enum BadgeKind
{
    Achievement,
    Shame
}

/// <summary>A badge that a member has received.</summary>
public class UserBadge : IEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string BadgeCode { get; set; } = string.Empty;
    public string BadgeName { get; set; } = string.Empty;
    public BadgeKind Kind { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateOnly AwardedOn { get; set; }

    /// <summary>Period evaluated (only for shame badges).</summary>
    public DateOnly? RangeStart { get; set; }
    public DateOnly? RangeEnd { get; set; }
}
