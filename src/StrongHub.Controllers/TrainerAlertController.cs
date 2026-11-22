using System.Globalization;
using StrongHub.Data;
using StrongHub.Models;

namespace StrongHub.Controllers;

/// <summary>How many alerts a refresh created.</summary>
public record AlertRefreshResult(int RoutineAlerts, int MeasurementAlerts)
{
    public int Total => RoutineAlerts + MeasurementAlerts;
}

/// <summary>
/// Creates the alerts of the logged-in trainer: routines about to expire and measurements that are due.
/// The number of days is configurable.
/// </summary>
public class TrainerAlertController
{
    public const int MinThresholdDays = 1;
    public const int MaxThresholdDays = 90;

    /// <summary>A member should be measured again this many days after the last measurement.</summary>
    public const int MeasurementIntervalDays = 28;

    private readonly NotificationCenter _center;
    private readonly IRoutineRepository _routines;
    private readonly IRepository<BasicUser> _users;
    private readonly IRepository<Measurement> _measurements;
    private readonly IAlertSettingsStore _store;
    private readonly SessionContext _session;
    private readonly Func<DateOnly> _today;
    private int _thresholdDays;

    public TrainerAlertController(
        NotificationCenter center,
        IRoutineRepository routines,
        IRepository<BasicUser> users,
        IRepository<Measurement> measurements,
        IAlertSettingsStore store,
        SessionContext session,
        Func<DateOnly>? today = null)
    {
        _center = center;
        _routines = routines;
        _users = users;
        _measurements = measurements;
        _store = store;
        _session = session;
        _today = today ?? (() => DateOnly.FromDateTime(DateTime.Today));
        _thresholdDays = Math.Clamp(store.LoadThresholdDays(), MinThresholdDays, MaxThresholdDays);
    }

    public bool CanConfigure => _session.CurrentUser is Trainer;

    public int ThresholdDays => _thresholdDays;

    /// <summary>Creates the alerts that do not exist yet. Members get nothing.</summary>
    public AlertRefreshResult Refresh()
    {
        if (_session.CurrentUser is not Trainer trainer)
        {
            return new AlertRefreshResult(0, 0);
        }

        DateOnly today = _today();
        DateOnly limit = today.AddDays(_thresholdDays);

        int routineAlerts = PublishRoutineAlerts(trainer, today, limit);
        int measurementAlerts = PublishMeasurementAlerts(trainer, limit);
        return new AlertRefreshResult(routineAlerts, measurementAlerts);
    }

    /// <summary>Saves a new threshold and checks the alerts again.</summary>
    public AlertRefreshResult SetThresholdAndRefresh(int days)
    {
        if (_session.CurrentUser is not Trainer)
        {
            throw new ValidationException("Only trainers can change the alert threshold.");
        }

        if (days < MinThresholdDays || days > MaxThresholdDays)
        {
            throw new ValidationException(
                $"The threshold must be between {MinThresholdDays} and {MaxThresholdDays} days.");
        }

        _store.SaveThresholdDays(days);
        _thresholdDays = days;
        return Refresh();
    }

    private int PublishRoutineAlerts(Trainer trainer, DateOnly today, DateOnly limit)
    {
        Dictionary<int, string> memberNames = _users.GetAll().ToDictionary(user => user.Id, user => user.Name);

        IEnumerable<Routine> expiring = _routines.GetAll()
            .Where(routine => routine.TrainerId == trainer.Id && routine.EndDate >= today && routine.EndDate <= limit)
            .OrderBy(routine => routine.EndDate)
            .ThenBy(routine => routine.Id);

        int created = 0;
        foreach (Routine routine in expiring)
        {
            string member = memberNames.GetValueOrDefault(routine.UserId, "Unknown member");
            string message = $"Routine '{routine.Name}' of {member} expires on {Day(routine.EndDate)}.";

            if (PublishOnce(trainer, NotificationType.RoutineExpiring, message))
            {
                created++;
            }
        }

        return created;
    }

    private int PublishMeasurementAlerts(Trainer trainer, DateOnly limit)
    {
        Dictionary<int, DateOnly> lastByMember = _measurements.GetAll()
            .GroupBy(measurement => measurement.UserId)
            .ToDictionary(group => group.Key, group => group.Max(measurement => measurement.Date));

        int created = 0;
        foreach (BasicUser member in _users.GetAll().Where(user => user.TrainerId == trainer.Id))
        {
            string? message = BuildMeasurementMessage(member, lastByMember, limit);

            if (message is not null && PublishOnce(trainer, NotificationType.MeasurementDue, message))
            {
                created++;
            }
        }

        return created;
    }

    private static string? BuildMeasurementMessage(
        BasicUser member,
        IReadOnlyDictionary<int, DateOnly> lastByMember,
        DateOnly limit)
    {
        if (!lastByMember.TryGetValue(member.Id, out DateOnly last))
        {
            return $"Measurement due: {member.Name} has no measurements yet.";
        }

        DateOnly due = last.AddDays(MeasurementIntervalDays);
        return due <= limit
            ? $"Measurement due: {member.Name} should be measured by {Day(due)} (last one: {Day(last)})."
            : null;
    }

    private bool PublishOnce(Trainer trainer, NotificationType type, string message)
    {
        if (_center.AlreadyPublished(trainer.Id, forTrainer: true, type, message))
        {
            return false;
        }

        _center.Publish(trainer.Id, type, message, forTrainer: true);
        return true;
    }

    private static string Day(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
