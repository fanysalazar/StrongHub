using System.Globalization;
using StrongHub.Data;
using StrongHub.Models;

namespace StrongHub.Controllers;

/// <summary>
/// Listens to the events of the controllers, writes the timeline messages and tells its own observers.
/// </summary>
public class NotificationCenter
{
    private readonly List<INotificationObserver> _observers = new();
    private readonly IRepository<Notification> _notifications;
    private readonly Func<DateTime> _now;

    public NotificationCenter(IRepository<Notification> notifications, Func<DateTime>? now = null)
    {
        _notifications = notifications;
        _now = now ?? (() => DateTime.Now);
    }

    public void Subscribe(INotificationObserver observer)
    {
        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
        }
    }

    public void Unsubscribe(INotificationObserver observer) => _observers.Remove(observer);

    /// <summary>Saves a notification and tells every observer.</summary>
    public Notification Publish(int userId, NotificationType type, string message, bool forTrainer = false)
    {
        var notification = new Notification
        {
            UserId = userId,
            ForTrainer = forTrainer,
            Type = type,
            Message = message,
            CreatedAt = _now()
        };

        _notifications.Add(notification);

        foreach (INotificationObserver observer in _observers.ToList())
        {
            observer.OnNotification(notification);
        }

        return notification;
    }

    public void ListenTo(RoutineController routines) => routines.RoutineCreated += HandleRoutineCreated;

    public void ListenTo(VisitController visits) => visits.RestRecommended += HandleRestRecommended;

    public void ListenTo(BadgeController badges) => badges.BadgeEarned += HandleBadgeEarned;

    public void HandleRoutineCreated(object? sender, RoutineCreatedEventArgs e)
    {
        Routine routine = e.Routine;
        string message = $"New routine '{routine.Name}' from {e.TrainerName}: {Day(routine.StartDate)} to {Day(routine.EndDate)}.";
        Publish(routine.UserId, NotificationType.NewRoutine, message);
    }

    public void HandleRestRecommended(object? sender, WeeklyRestEventArgs e)
    {
        string message = $"Rest this week! You reached {e.Visits} visits in the week of {Day(e.WeekStart)}.";
        Publish(e.UserId, NotificationType.RestThisWeek, message);
    }

    public void HandleBadgeEarned(object? sender, BadgeEarnedEventArgs e)
    {
        string message = $"Badge earned: {e.Badge.BadgeName}. {e.Badge.Message}";
        Publish(e.UserId, NotificationType.BadgeEarned, message);
    }

    private static string Day(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
