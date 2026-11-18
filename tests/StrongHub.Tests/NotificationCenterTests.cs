using StrongHub.Controllers;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class NotificationCenterTests
{
    private static readonly DateTime Moment = new(2026, 11, 18, 9, 30, 0);

    private readonly InMemoryRepository<Notification> _notifications = new();
    private readonly NotificationCenter _center;

    public NotificationCenterTests()
    {
        _center = new NotificationCenter(_notifications, () => Moment);
    }

    private sealed class FakeObserver : INotificationObserver
    {
        public List<Notification> Received { get; } = new();

        public void OnNotification(Notification notification) => Received.Add(notification);
    }

    [Fact]
    public void Publish_SavesTheNotificationWithTheCurrentTimeAndNotifiesTheObserver()
    {
        var observer = new FakeObserver();
        _center.Subscribe(observer);

        Notification published = _center.Publish(3, NotificationType.BadgeEarned, "Hello");

        Assert.Single(_notifications.GetAll());
        Assert.Equal(Moment, published.CreatedAt);
        Assert.Equal(3, published.UserId);
        Assert.False(published.ForTrainer);
        Assert.Same(published, Assert.Single(observer.Received));
    }

    [Fact]
    public void Publish_TwoObservers_BothAreNotified()
    {
        var first = new FakeObserver();
        var second = new FakeObserver();
        _center.Subscribe(first);
        _center.Subscribe(second);

        _center.Publish(1, NotificationType.NewRoutine, "Hello");

        Assert.Single(first.Received);
        Assert.Single(second.Received);
    }

    [Fact]
    public void Unsubscribe_TheObserverStopsReceiving()
    {
        var observer = new FakeObserver();
        _center.Subscribe(observer);
        _center.Unsubscribe(observer);

        _center.Publish(1, NotificationType.NewRoutine, "Hello");

        Assert.Empty(observer.Received);
    }

    [Fact]
    public void Subscribe_TheSameObserverTwice_IsNotifiedOnlyOnce()
    {
        var observer = new FakeObserver();
        _center.Subscribe(observer);
        _center.Subscribe(observer);

        _center.Publish(1, NotificationType.NewRoutine, "Hello");

        Assert.Single(observer.Received);
    }

    [Fact]
    public void HandleRoutineCreated_PublishesANewRoutineMessageForTheMember()
    {
        var routine = new Routine
        {
            UserId = 4,
            Name = "Strength block",
            StartDate = new DateOnly(2026, 11, 20),
            EndDate = new DateOnly(2026, 12, 18)
        };

        _center.HandleRoutineCreated(this, new RoutineCreatedEventArgs(routine, "Bryan Cordero", "Marisela Aguilar"));

        Notification saved = Assert.Single(_notifications.GetAll());
        Assert.Equal(4, saved.UserId);
        Assert.Equal(NotificationType.NewRoutine, saved.Type);
        Assert.Equal("New routine 'Strength block' from Marisela Aguilar: 2026-11-20 to 2026-12-18.", saved.Message);
    }

    [Fact]
    public void HandleRestRecommended_PublishesARestMessageForTheMember()
    {
        _center.HandleRestRecommended(this, new WeeklyRestEventArgs(2, "Xinia Segura", new DateOnly(2026, 11, 16), 5));

        Notification saved = Assert.Single(_notifications.GetAll());
        Assert.Equal(2, saved.UserId);
        Assert.Equal(NotificationType.RestThisWeek, saved.Type);
        Assert.Equal("Rest this week! You reached 5 visits in the week of 2026-11-16.", saved.Message);
    }

    [Fact]
    public void HandleBadgeEarned_PublishesTheBadgeNameAndMessage()
    {
        var badge = new UserBadge { UserId = 3, BadgeName = "Regular visitor", Message = "You attended 6 or more classes in 30 days." };

        _center.HandleBadgeEarned(this, new BadgeEarnedEventArgs(3, "Greivin Quesada", badge));

        Notification saved = Assert.Single(_notifications.GetAll());
        Assert.Equal(3, saved.UserId);
        Assert.Equal(NotificationType.BadgeEarned, saved.Type);
        Assert.Equal("Badge earned: Regular visitor. You attended 6 or more classes in 30 days.", saved.Message);
    }

    [Fact]
    public void ListenTo_RoutineController_CreatingARoutineNotifiesTheMember()
    {
        var users = new InMemoryRepository<BasicUser>();
        users.Add(new BasicUser { Name = "Bryan Cordero", Username = "bryan" });
        var session = new SessionContext();
        session.SignIn(new Trainer { Id = 7, Name = "Marisela Aguilar", Username = "marisela" });
        var routineController = new RoutineController(
            new InMemoryRoutineRepository(), users, session, new InMemoryRepository<RoutineChange>());
        _center.ListenTo(routineController);

        routineController.Create(
            1,
            "Strength block",
            new DateOnly(2026, 11, 20),
            new DateOnly(2026, 12, 18),
            new List<Exercise> { new() { Name = "Squat", Sets = 4, Reps = 8, WeightKg = 60 } });

        Notification saved = Assert.Single(_notifications.GetAll());
        Assert.Equal(1, saved.UserId);
        Assert.Equal(NotificationType.NewRoutine, saved.Type);
        Assert.Contains("Strength block", saved.Message);
    }
}
