using StrongHub.Controllers;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class ShameNotificationTests
{
    private static readonly DateOnly Today = new(2026, 11, 18);

    private readonly InMemoryRepository<Notification> _notifications = new();
    private readonly NotificationCenter _center;

    public ShameNotificationTests()
    {
        _center = new NotificationCenter(_notifications, () => new DateTime(2026, 11, 18, 9, 0, 0));
    }

    [Fact]
    public void HandleShameBadgeAssigned_PublishesAShameMessageWithThePeriodForTheMember()
    {
        var badge = new UserBadge
        {
            UserId = 5,
            BadgeName = "Fewest visits",
            Message = "You are among the 10 members with the fewest visits.",
            RangeStart = new DateOnly(2026, 11, 1),
            RangeEnd = Today
        };

        _center.HandleShameBadgeAssigned(this, new ShameBadgeEventArgs(5, "Yorleny Bonilla", badge));

        Notification saved = Assert.Single(_notifications.GetAll());
        Assert.Equal(5, saved.UserId);
        Assert.False(saved.ForTrainer);
        Assert.Equal(NotificationType.ShameBadge, saved.Type);
        Assert.Equal(
            "Shame badge: Fewest visits (2026-11-01 to 2026-11-18). You are among the 10 members with the fewest visits.",
            saved.Message);
    }

    [Fact]
    public void HandleShameBadgeAssigned_WithoutPeriod_OmitsIt()
    {
        var badge = new UserBadge { UserId = 5, BadgeName = "Goals not met", Message = "Keep trying!" };

        _center.HandleShameBadgeAssigned(this, new ShameBadgeEventArgs(5, "Yorleny Bonilla", badge));

        Assert.Equal("Shame badge: Goals not met. Keep trying!", Assert.Single(_notifications.GetAll()).Message);
    }

    [Fact]
    public void ListenTo_ShameBadgeController_EveryAssignedBadgeBecomesATimelineMessage()
    {
        ShameBadgeController controller = CreateShameController(out SessionContext session);
        session.SignIn(new Trainer { Id = 9, Name = "Eliecer Fonseca", Username = "eliecer" });
        _center.ListenTo(controller);

        controller.Evaluate(new DateOnly(2026, 11, 1), Today);

        Assert.Equal(3, _notifications.GetAll().Count);
        Assert.All(_notifications.GetAll(), notification =>
        {
            Assert.Equal(NotificationType.ShameBadge, notification.Type);
            Assert.False(notification.ForTrainer);
        });
        Assert.Equal(new[] { 1, 2, 3 }, _notifications.GetAll().Select(notification => notification.UserId).OrderBy(id => id));
    }

    [Fact]
    public void ListenTo_ShameBadgeController_EvaluatingTheSamePeriodTwiceDoesNotRepeatMessages()
    {
        ShameBadgeController controller = CreateShameController(out SessionContext session);
        session.SignIn(new Trainer { Id = 9, Name = "Eliecer Fonseca", Username = "eliecer" });
        _center.ListenTo(controller);

        controller.Evaluate(new DateOnly(2026, 11, 1), Today);
        controller.Evaluate(new DateOnly(2026, 11, 1), Today);

        Assert.Equal(3, _notifications.GetAll().Count);
    }

    [Fact]
    public void AlreadyPublished_MatchesOnlyTheSameRecipientTypeAndText()
    {
        _center.Publish(7, NotificationType.RoutineExpiring, "Same text", forTrainer: true);

        Assert.True(_center.AlreadyPublished(7, true, NotificationType.RoutineExpiring, "Same text"));
        Assert.False(_center.AlreadyPublished(7, true, NotificationType.RoutineExpiring, "Other text"));
        Assert.False(_center.AlreadyPublished(7, false, NotificationType.RoutineExpiring, "Same text"));
        Assert.False(_center.AlreadyPublished(8, true, NotificationType.RoutineExpiring, "Same text"));
        Assert.False(_center.AlreadyPublished(7, true, NotificationType.MeasurementDue, "Same text"));
    }

    private static ShameBadgeController CreateShameController(out SessionContext session)
    {
        var users = new InMemoryRepository<BasicUser>();
        for (int id = 1; id <= 3; id++)
        {
            users.Add(new BasicUser { Name = $"Tester {id}", Username = $"tester{id}" });
        }

        var visits = new InMemoryVisitRepository();
        var measurements = new InMemoryRepository<Measurement>();
        session = new SessionContext();

        return new ShameBadgeController(
            users,
            new InMemoryRepository<UserBadge>(),
            visits,
            new BadgeEvaluator(new IBadgeRule[] { new LowestVisitsRule() }),
            new BadgeContextFactory(measurements, visits),
            session,
            () => Today);
    }
}
