using StrongHub.Controllers;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class TimelineControllerTests
{
    private readonly InMemoryRepository<Notification> _notifications = new();
    private readonly UnseenNotificationTracker _tracker = new();
    private readonly SessionContext _session = new();
    private readonly TimelineController _controller;

    public TimelineControllerTests()
    {
        _controller = new TimelineController(_notifications, _tracker, _session);
    }

    private void Add(int userId, bool forTrainer, DateTime when, string message,
        NotificationType type = NotificationType.BadgeEarned) =>
        _notifications.Add(new Notification
        {
            UserId = userId, ForTrainer = forTrainer, CreatedAt = when, Message = message, Type = type
        });

    private void SignInMember(int id = 1) =>
        _session.SignIn(new BasicUser { Id = id, Name = "Nazareth Montero", Username = "nazareth" });

    [Fact]
    public void GetRows_Member_ReturnsOnlyOwnMessagesNewestFirst()
    {
        Add(1, false, new DateTime(2026, 11, 10, 8, 0, 0), "older");
        Add(1, false, new DateTime(2026, 11, 18, 9, 30, 0), "newer", NotificationType.RestThisWeek);
        Add(2, false, new DateTime(2026, 11, 19, 8, 0, 0), "someone else");
        SignInMember();

        IReadOnlyList<TimelineRow> rows = _controller.GetRows();

        Assert.Equal(2, rows.Count);
        Assert.Equal("newer", rows[0].Message);
        Assert.Equal("2026-11-18 09:30", rows[0].When);
        Assert.Equal("Rest", rows[0].Type);
    }

    [Fact]
    public void GetRows_Member_IgnoresTrainerMessagesWithTheSameId()
    {
        Add(1, true, new DateTime(2026, 11, 18, 9, 0, 0), "for the trainer");
        SignInMember(1);

        Assert.Empty(_controller.GetRows());
    }

    [Fact]
    public void GetRows_Trainer_ReturnsOnlyTrainerMessages()
    {
        Add(1, true, new DateTime(2026, 11, 18, 9, 0, 0), "for the trainer", NotificationType.RoutineExpiring);
        Add(1, false, new DateTime(2026, 11, 18, 10, 0, 0), "for the member");
        _session.SignIn(new Trainer { Id = 1, Name = "Eliecer Villegas", Username = "eliecer" });

        TimelineRow row = Assert.Single(_controller.GetRows());

        Assert.Equal("for the trainer", row.Message);
        Assert.Equal("Routine expiring", row.Type);
    }

    [Fact]
    public void GetRows_NobodySignedIn_ReturnsNothing()
    {
        Add(1, false, new DateTime(2026, 11, 18, 9, 0, 0), "hello");

        Assert.Empty(_controller.GetRows());
        Assert.Equal(0, _controller.GetUnseenCount());
    }

    [Fact]
    public void GetSummary_WithoutMessages_SaysSo()
    {
        SignInMember();

        Assert.Equal("No notifications yet.", _controller.GetSummary());
    }

    [Fact]
    public void GetSummary_WithMessages_CountsThem()
    {
        Add(1, false, new DateTime(2026, 11, 10, 8, 0, 0), "one");
        Add(1, false, new DateTime(2026, 11, 11, 8, 0, 0), "two");
        SignInMember();

        Assert.Equal("2 notification(s), newest first.", _controller.GetSummary());
    }

    [Fact]
    public void Unseen_CountsPerRecipientAndResetsWhenMarkedSeen()
    {
        _tracker.OnNotification(new Notification { UserId = 1, ForTrainer = false });
        _tracker.OnNotification(new Notification { UserId = 1, ForTrainer = false });
        _tracker.OnNotification(new Notification { UserId = 1, ForTrainer = true });
        SignInMember(1);

        Assert.Equal(2, _controller.GetUnseenCount());

        _controller.MarkAllSeen();
        Assert.Equal(0, _controller.GetUnseenCount());

        _session.SignIn(new Trainer { Id = 1, Name = "Eliecer Villegas", Username = "eliecer" });
        Assert.Equal(1, _controller.GetUnseenCount());
    }
}
