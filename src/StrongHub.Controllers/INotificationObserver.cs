using StrongHub.Models;

namespace StrongHub.Controllers;

/// <summary>Observer: anything that wants to know when a notification is published.</summary>
public interface INotificationObserver
{
    void OnNotification(Notification notification);
}
