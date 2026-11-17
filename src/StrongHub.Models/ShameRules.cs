namespace StrongHub.Models;

/// <summary>The member is one of the 10 with the fewest visits in the period.</summary>
public class LowestVisitsRule : IBadgeRule
{
    public string Code => "fewest-visits";
    public string Name => "Fewest visits";
    public BadgeKind Kind => BadgeKind.Shame;
    public string Message => "You are among the 10 members with the fewest visits. Come back, we miss you!";

    public bool IsSatisfied(BadgeContext context) => context.LowestVisitorIds.Contains(context.Member.Id);
}

/// <summary>
/// No progress in the period: fewer than two measurements, or the last one shows
/// neither more muscle nor less body fat than the first one.
/// </summary>
public class NoProgressRule : IBadgeRule
{
    public string Code => "no-progress";
    public string Name => "No progress registered";
    public BadgeKind Kind => BadgeKind.Shame;
    public string Message => "No progress was registered in this period. Let's set a new plan!";

    public bool IsSatisfied(BadgeContext context)
    {
        IReadOnlyList<Measurement> measurements = context.MeasurementsIn(context.RequireRange());
        if (measurements.Count < 2)
        {
            return true;
        }

        Measurement first = measurements[0];
        Measurement last = measurements[^1];
        bool improved = last.MuscleKg > first.MuscleKg || last.BodyFatPercent < first.BodyFatPercent;
        return !improved;
    }
}

/// <summary>Neither the muscle goal nor the body fat goal was met by the last measurement of the period.</summary>
public class GoalsNotMetRule : IBadgeRule
{
    public string Code => "goals-not-met";
    public string Name => "Goals not met";
    public BadgeKind Kind => BadgeKind.Shame;
    public string Message => "You did not reach your goals in this period. Keep trying!";

    public bool IsSatisfied(BadgeContext context)
    {
        Measurement? latest = context.LatestMeasurement(context.RequireRange());
        if (latest is null)
        {
            return true;
        }

        bool muscleGoalMet = latest.MuscleKg >= context.Member.GoalMuscleKg;
        bool bodyFatGoalMet = latest.BodyFatPercent <= context.Member.GoalBodyFatPercent;
        return !muscleGoalMet && !bodyFatGoalMet;
    }
}
