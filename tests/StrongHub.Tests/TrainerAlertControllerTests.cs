using StrongHub.Controllers;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class TrainerAlertControllerTests
{
    private static readonly DateOnly Today = new(2026, 11, 21);

    private readonly InMemoryRepository<Notification> _notifications = new();
    private readonly InMemoryRoutineRepository _routines = new();
    private readonly InMemoryRepository<BasicUser> _users = new();
    private readonly InMemoryRepository<Measurement> _measurements = new();
    private readonly FakeAlertSettingsStore _store = new() { Days = 7 };
    private readonly SessionContext _session = new();
    private readonly UnseenNotificationTracker _tracker = new();
    private readonly NotificationCenter _center;
    private readonly TrainerAlertController _controller;

    public TrainerAlertControllerTests()
    {
        _center = new NotificationCenter(_notifications, () => new DateTime(2026, 11, 21, 8, 0, 0));
        _center.Subscribe(_tracker);

        _users.Add(new BasicUser { Name = "Tester Uno", Username = "uno", TrainerId = 7 });
        _users.Add(new BasicUser { Name = "Tester Dos", Username = "dos", TrainerId = 3 });
        _session.SignIn(new Trainer { Id = 7, Name = "Eliecer Fonseca", Username = "eliecer" });

        _controller = CreateController();
    }

    private TrainerAlertController CreateController() =>
        new(_center, _routines, _users, _measurements, _store, _session, () => Today);

    private void AddRoutine(string name, DateOnly end, int userId = 1, int trainerId = 7) =>
        _routines.Add(new Routine
        {
            UserId = userId, TrainerId = trainerId, Name = name, StartDate = end.AddDays(-28), EndDate = end
        });

    private void AddMeasurement(int userId, DateOnly date) =>
        _measurements.Add(new Measurement { UserId = userId, Date = date });

    [Fact]
    public void Refresh_RoutineEndingInsideTheThreshold_CreatesATrainerAlert()
    {
        AddRoutine("Block A", new DateOnly(2026, 11, 25));

        AlertRefreshResult result = _controller.Refresh();

        Assert.Equal(1, result.RoutineAlerts);
        Notification alert = Assert.Single(_notifications.GetAll(), n => n.Type == NotificationType.RoutineExpiring);
        Assert.Equal(7, alert.UserId);
        Assert.True(alert.ForTrainer);
        Assert.Equal("Routine 'Block A' of Tester Uno expires on 2026-11-25.", alert.Message);
    }

    [Theory]
    [InlineData(2026, 11, 21, 1)]
    [InlineData(2026, 11, 28, 1)]
    [InlineData(2026, 11, 29, 0)]
    [InlineData(2026, 11, 20, 0)]
    public void Refresh_RoutineEndDate_IsOnlyAlertedBetweenTodayAndTheLimit(int year, int month, int day, int expected)
    {
        AddRoutine("Block A", new DateOnly(year, month, day));

        Assert.Equal(expected, _controller.Refresh().RoutineAlerts);
    }

    [Fact]
    public void Refresh_RoutineOfAnotherTrainer_IsNotAlerted()
    {
        AddRoutine("Block A", new DateOnly(2026, 11, 25), userId: 2, trainerId: 3);

        Assert.Equal(0, _controller.Refresh().RoutineAlerts);
    }

    [Fact]
    public void Refresh_LastMeasurementDueInsideTheThreshold_CreatesAnAlert()
    {
        AddMeasurement(1, new DateOnly(2026, 10, 31));

        AlertRefreshResult result = _controller.Refresh();

        Assert.Equal(1, result.MeasurementAlerts);
        Notification alert = Assert.Single(_notifications.GetAll(), n => n.Type == NotificationType.MeasurementDue);
        Assert.Equal(
            "Measurement due: Tester Uno should be measured by 2026-11-28 (last one: 2026-10-31).",
            alert.Message);
    }

    [Fact]
    public void Refresh_MeasurementNotDueYet_CreatesNothing()
    {
        AddMeasurement(1, new DateOnly(2026, 11, 1));

        Assert.Equal(0, _controller.Refresh().MeasurementAlerts);
    }

    [Fact]
    public void Refresh_OverdueMeasurement_IsAlerted()
    {
        AddMeasurement(1, new DateOnly(2026, 9, 1));

        Assert.Equal(1, _controller.Refresh().MeasurementAlerts);
    }

    [Fact]
    public void Refresh_MemberWithoutMeasurements_IsAlerted()
    {
        _controller.Refresh();

        Notification alert = Assert.Single(_notifications.GetAll(), n => n.Type == NotificationType.MeasurementDue);
        Assert.Equal("Measurement due: Tester Uno has no measurements yet.", alert.Message);
    }

    [Fact]
    public void Refresh_OnlyTheLatestMeasurementCounts()
    {
        AddMeasurement(1, new DateOnly(2026, 9, 1));
        AddMeasurement(1, new DateOnly(2026, 11, 15));

        Assert.Equal(0, _controller.Refresh().MeasurementAlerts);
    }

    [Fact]
    public void Refresh_MemberOfAnotherTrainer_IsNotAlerted()
    {
        AddMeasurement(1, new DateOnly(2026, 11, 15));

        Assert.Equal(0, _controller.Refresh().MeasurementAlerts);
    }

    [Fact]
    public void Refresh_CalledTwice_DoesNotRepeatTheAlerts()
    {
        AddRoutine("Block A", new DateOnly(2026, 11, 25));
        _controller.Refresh();
        int saved = _notifications.GetAll().Count;

        AlertRefreshResult second = _controller.Refresh();

        Assert.Equal(0, second.Total);
        Assert.Equal(saved, _notifications.GetAll().Count);
    }

    [Fact]
    public void Refresh_TheUnseenCounterOfTheTrainerGrowsWithTheAlerts()
    {
        AddRoutine("Block A", new DateOnly(2026, 11, 22));
        AddRoutine("Block B", new DateOnly(2026, 11, 24));

        AlertRefreshResult result = _controller.Refresh();

        Assert.Equal(3, result.Total);
        Assert.Equal(3, _tracker.CountFor(7, true));
    }

    [Fact]
    public void Refresh_SignedInAsMember_CreatesNothing()
    {
        AddRoutine("Block A", new DateOnly(2026, 11, 25));
        _session.SignIn(_users.GetAll()[0]);

        Assert.Equal(0, _controller.Refresh().Total);
        Assert.Empty(_notifications.GetAll());
        Assert.False(_controller.CanConfigure);
    }

    [Fact]
    public void SetThresholdAndRefresh_ABiggerThreshold_SavesItAndFindsMoreAlerts()
    {
        AddRoutine("Late block", new DateOnly(2026, 12, 5));
        Assert.Equal(0, _controller.Refresh().RoutineAlerts);

        AlertRefreshResult result = _controller.SetThresholdAndRefresh(30);

        Assert.Equal(1, result.RoutineAlerts);
        Assert.Equal(30, _store.Days);
        Assert.Equal(30, _controller.ThresholdDays);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(91)]
    public void SetThresholdAndRefresh_OutOfRange_ThrowsAndKeepsTheOldValue(int days)
    {
        Assert.Throws<ValidationException>(() => _controller.SetThresholdAndRefresh(days));

        Assert.Equal(7, _store.Days);
        Assert.Equal(7, _controller.ThresholdDays);
    }

    [Fact]
    public void SetThresholdAndRefresh_AsMember_ThrowsValidationException()
    {
        _session.SignIn(_users.GetAll()[0]);

        Assert.Throws<ValidationException>(() => _controller.SetThresholdAndRefresh(10));
    }

    [Theory]
    [InlineData(500, 90)]
    [InlineData(0, 1)]
    public void Constructor_StoredValueOutOfRange_IsClamped(int stored, int expected)
    {
        _store.Days = stored;

        Assert.Equal(expected, CreateController().ThresholdDays);
    }
}
