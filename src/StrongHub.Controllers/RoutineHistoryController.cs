using System.Globalization;
using StrongHub.Data;
using StrongHub.Models;

namespace StrongHub.Controllers;

/// <summary>One row of the "latest routines" tab.</summary>
public record MyRoutineRow(string Name, string Trainer, string Start, string End, string Status, string Exercises);

/// <summary>Provides the most recent routines of the logged-in member.</summary>
public class RoutineHistoryController
{
    public const int LatestCount = 5;

    private readonly IRoutineRepository _routines;
    private readonly IRepository<Trainer> _trainers;
    private readonly SessionContext _session;
    private readonly Func<DateOnly> _today;

    public RoutineHistoryController(
        IRoutineRepository routines,
        IRepository<Trainer> trainers,
        SessionContext session,
        Func<DateOnly>? today = null)
    {
        _routines = routines;
        _trainers = trainers;
        _session = session;
        _today = today ?? (() => DateOnly.FromDateTime(DateTime.Today));
    }

    /// <summary>The 5 routines with the latest start date (never more). Trainers get an empty list.</summary>
    public IReadOnlyList<MyRoutineRow> GetLatestRows()
    {
        if (_session.CurrentUser is not BasicUser member)
        {
            return Array.Empty<MyRoutineRow>();
        }

        Dictionary<int, string> trainerNames = _trainers.GetAll().ToDictionary(trainer => trainer.Id, trainer => trainer.Name);
        DateOnly today = _today();

        return _routines.GetByUser(member.Id)
            .Take(LatestCount)
            .Select(routine => new MyRoutineRow(
                routine.Name,
                trainerNames.GetValueOrDefault(routine.TrainerId, "Unknown trainer"),
                Day(routine.StartDate),
                Day(routine.EndDate),
                StatusOf(routine, today),
                Describe(routine.Exercises)))
            .ToList();
    }

    public string GetSummary()
    {
        if (_session.CurrentUser is not BasicUser member)
        {
            return "Routines are available for members.";
        }

        int total = _routines.GetByUser(member.Id).Count;

        if (total == 0)
        {
            return "You have no routines yet.";
        }

        return total <= LatestCount
            ? $"{total} routine(s), newest first."
            : $"Latest {LatestCount} of {total} routines, newest first.";
    }

    private static string StatusOf(Routine routine, DateOnly today)
    {
        if (today < routine.StartDate)
        {
            return "Upcoming";
        }

        return today > routine.EndDate ? "Finished" : "Active";
    }

    private static string Describe(IEnumerable<Exercise> exercises) =>
        string.Join("; ", exercises.Select(exercise =>
            $"{exercise.Name} {exercise.Sets}x{exercise.Reps} @ {exercise.WeightKg.ToString("0.##", CultureInfo.InvariantCulture)} kg"));

    private static string Day(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
