using System.Globalization;
using StrongHub.Data;
using StrongHub.Models;

namespace StrongHub.Controllers;

/// <summary>One row of the timeline shown in the view.</summary>
public record TimelineRow(string When, string Type, string Message);

/// <summary>Provides the timeline of the logged-in member or trainer, newest first.</summary>
public class TimelineController
{
    private readonly IRepository<Notification> _notifications;
    private readonly UnseenNotificationTracker _tracker;
    private readonly SessionContext _session;

    public TimelineController(
        IRepository<Notification> notifications,
        UnseenNotificationTracker tracker,
        SessionContext session)
    {
        _notifications = notifications;
        _tracker = tracker;
        _session = session;
    }

    public IReadOnlyList<TimelineRow> GetRows()
    {
        (int Id, bool ForTrainer)? recipient = CurrentRecipient();
        if (recipient is null)
        {
            return Array.Empty<TimelineRow>();
        }

        return _notifications.GetAll()
            .Where(notification => notification.UserId == recipient.Value.Id
                                   && notification.ForTrainer == recipient.Value.ForTrainer)
            .OrderByDescending(notification => notification.CreatedAt)
            .ThenByDescending(notification => notification.Id)
            .Select(notification => new TimelineRow(
                notification.CreatedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                Label(notification.Type),
                notification.Message))
            .ToList();
    }

    public string GetSummary()
    {
        int count = GetRows().Count;
        return count == 0 ? "No notifications yet." : $"{count} notification(s), newest first.";
    }

    /// <summary>Notifications received since the program started and not opened yet.</summary>
    public int GetUnseenCount() =>
        CurrentRecipient() is { } recipient ? _tracker.CountFor(recipient.Id, recipient.ForTrainer) : 0;

    public void MarkAllSeen()
    {
        if (CurrentRecipient() is { } recipient)
        {
            _tracker.MarkSeen(recipient.Id, recipient.ForTrainer);
        }
    }

    private (int Id, bool ForTrainer)? CurrentRecipient()
    {
        return _session.CurrentUser switch
        {
            BasicUser member => (member.Id, false),
            Trainer trainer => (trainer.Id, true),
            _ => null
        };
    }

    private static string Label(NotificationType type) => type switch
    {
        NotificationType.BadgeEarned => "Badge",
        NotificationType.NewRoutine => "New routine",
        NotificationType.RestThisWeek => "Rest",
        NotificationType.RoutineExpiring => "Routine expiring",
        NotificationType.MeasurementDue => "Measurement",
        NotificationType.ShameBadge => "Shame badge",
        _ => type.ToString()
    };
}
