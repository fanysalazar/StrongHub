namespace StrongHub.Models;

/// <summary>Kinds of messages that can appear in a timeline.</summary>
public enum NotificationType
{
    BadgeEarned,
    NewRoutine,
    RestThisWeek,
    RoutineExpiring,
    MeasurementDue,
    ShameBadge
}

/// <summary>One message of a timeline. Member and trainer ids overlap, so ForTrainer tells them apart.</summary>
public class Notification : IEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public bool ForTrainer { get; set; }
    public NotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
