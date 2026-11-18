using StrongHub.Models;

namespace StrongHub.Controllers;

/// <summary>Observer of the notification center: counts what has not been opened yet in this session.</summary>
public class UnseenNotificationTracker : INotificationObserver
{
    private readonly Dictionary<(int Id, bool ForTrainer), int> _unseen = new();

    public void OnNotification(Notification notification)
    {
        (int, bool) key = (notification.UserId, notification.ForTrainer);
        _unseen[key] = CountFor(notification.UserId, notification.ForTrainer) + 1;
    }

    public int CountFor(int id, bool forTrainer) =>
        _unseen.TryGetValue((id, forTrainer), out int count) ? count : 0;

    public void MarkSeen(int id, bool forTrainer) => _unseen.Remove((id, forTrainer));
}
