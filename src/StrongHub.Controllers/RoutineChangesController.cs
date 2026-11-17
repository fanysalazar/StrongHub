using System.Globalization;
using StrongHub.Data;
using StrongHub.Models;

namespace StrongHub.Controllers;

/// <summary>One row of the load history shown in the view.</summary>
public record RoutineChangeRow(string Date, string Routine, string Exercise, double PreviousKg, double NewKg, string Change);

/// <summary>Provides the history of load changes of the member who is logged in.</summary>
public class RoutineChangesController
{
    private readonly IRepository<RoutineChange> _changes;
    private readonly IRepository<Routine> _routines;
    private readonly SessionContext _session;

    public RoutineChangesController(
        IRepository<RoutineChange> changes,
        IRepository<Routine> routines,
        SessionContext session)
    {
        _changes = changes;
        _routines = routines;
        _session = session;
    }

    /// <summary>Changes of the current member, newest first. Trainers get an empty list.</summary>
    public IReadOnlyList<RoutineChangeRow> GetRows()
    {
        if (_session.CurrentUser is not BasicUser member)
        {
            return Array.Empty<RoutineChangeRow>();
        }

        Dictionary<int, string> routineNames = _routines.GetAll().ToDictionary(routine => routine.Id, routine => routine.Name);

        return _changes.GetAll()
            .Where(change => change.UserId == member.Id)
            .OrderByDescending(change => change.Date)
            .ThenByDescending(change => change.Id)
            .Select(change => new RoutineChangeRow(
                change.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                routineNames.GetValueOrDefault(change.RoutineId, "(deleted routine)"),
                change.ExerciseName,
                change.OldWeightKg,
                change.NewWeightKg,
                FormatDifference(change.NewWeightKg - change.OldWeightKg)))
            .ToList();
    }

    /// <summary>Short text with the number of increases and the total weight gained.</summary>
    public string GetSummary()
    {
        if (_session.CurrentUser is not BasicUser member)
        {
            return "Load history is available for members.";
        }

        List<RoutineChange> increases = _changes.GetAll()
            .Where(change => change.UserId == member.Id && change.NewWeightKg > change.OldWeightKg)
            .ToList();

        if (increases.Count == 0)
        {
            return "No load increases recorded yet.";
        }

        double total = increases.Sum(change => change.NewWeightKg - change.OldWeightKg);
        return $"{increases.Count} load increase(s) recorded, +{total.ToString("0.##", CultureInfo.InvariantCulture)} kg in total";
    }

    private static string FormatDifference(double difference) =>
        Math.Round(difference, 2).ToString("+0.##;-0.##;0", CultureInfo.InvariantCulture);
}
