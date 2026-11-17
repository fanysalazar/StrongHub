using StrongHub.Models;

namespace StrongHub.Controllers;

/// <summary>Finds which exercises changed their weight when a routine is edited.</summary>
public static class RoutineChangeDetector
{
    private const double Tolerance = 0.0001;

    public static List<RoutineChange> Detect(
        IReadOnlyList<Exercise> before,
        IReadOnlyList<Exercise> after,
        int routineId,
        int userId,
        DateOnly date)
    {
        var changes = new List<RoutineChange>();
        List<Exercise> unmatched = before.ToList();

        foreach (Exercise current in after)
        {
            Exercise? previous = unmatched.FirstOrDefault(exercise =>
                string.Equals(exercise.Name, current.Name, StringComparison.OrdinalIgnoreCase));

            if (previous is null)
            {
                continue;
            }

            unmatched.Remove(previous);

            if (Math.Abs(previous.WeightKg - current.WeightKg) > Tolerance)
            {
                changes.Add(new RoutineChange
                {
                    RoutineId = routineId,
                    UserId = userId,
                    ExerciseName = current.Name,
                    Date = date,
                    OldWeightKg = previous.WeightKg,
                    NewWeightKg = current.WeightKg
                });
            }
        }

        return changes;
    }
}
