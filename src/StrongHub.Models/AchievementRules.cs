namespace StrongHub.Models;

/// <summary>The latest measurement reaches the muscle mass goal.</summary>
public class MuscleGoalRule : IBadgeRule
{
    public string Code => "muscle-goal";
    public string Name => "Muscle goal reached";
    public BadgeKind Kind => BadgeKind.Achievement;
    public string Message => "You reached your muscle mass goal. Great work!";

    public bool IsSatisfied(BadgeContext context)
    {
        Measurement? latest = context.LatestMeasurement();
        return latest is not null && latest.MuscleKg >= context.Member.GoalMuscleKg;
    }
}

/// <summary>The latest measurement reaches the body fat goal (at or below the target).</summary>
public class BodyFatGoalRule : IBadgeRule
{
    public string Code => "body-fat-goal";
    public string Name => "Body fat goal reached";
    public BadgeKind Kind => BadgeKind.Achievement;
    public string Message => "You reached your body fat goal. Keep it up!";

    public bool IsSatisfied(BadgeContext context)
    {
        Measurement? latest = context.LatestMeasurement();
        return latest is not null && latest.BodyFatPercent <= context.Member.GoalBodyFatPercent;
    }
}

/// <summary>A minimum number of visits in the last N days. Used with different numbers for different badges.</summary>
public class VisitCountRule : IBadgeRule
{
    private readonly int _minVisits;
    private readonly int _windowDays;

    public VisitCountRule(string code, string name, int minVisits, int windowDays)
    {
        Code = code;
        Name = name;
        _minVisits = minVisits;
        _windowDays = windowDays;
    }

    public string Code { get; }
    public string Name { get; }
    public BadgeKind Kind => BadgeKind.Achievement;
    public string Message => $"You attended {_minVisits} or more classes in {_windowDays} days.";

    public bool IsSatisfied(BadgeContext context)
    {
        var window = new DateRange(context.Today.AddDays(-(_windowDays - 1)), context.Today);
        return context.VisitsIn(window) >= _minVisits;
    }
}
