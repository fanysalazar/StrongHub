using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class ShameRulesTests
{
    private static readonly DateOnly Today = new(2026, 11, 18);
    private static readonly DateRange Range = new(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 18));

    private static Measurement Meas(int day, double muscle, double fat) =>
        new() { UserId = 1, Date = new DateOnly(2026, 11, day), MuscleKg = muscle, BodyFatPercent = fat };

    private static BadgeContext Context(IReadOnlyList<Measurement>? measurements = null, IReadOnlySet<int>? lowest = null) =>
        new(
            new BasicUser { Id = 1, Name = "Yorleny Bonilla", Username = "yorleny", GoalMuscleKg = 40, GoalBodyFatPercent = 18 },
            measurements ?? new List<Measurement>(),
            new List<Visit>(),
            Today,
            Range,
            lowest ?? new HashSet<int>());

    [Fact]
    public void LowestVisitsRule_MemberInTheRanking_IsSatisfied()
    {
        Assert.True(new LowestVisitsRule().IsSatisfied(Context(lowest: new HashSet<int> { 1 })));
    }

    [Fact]
    public void LowestVisitsRule_MemberOutsideTheRanking_IsNotSatisfied()
    {
        Assert.False(new LowestVisitsRule().IsSatisfied(Context(lowest: new HashSet<int> { 2, 3 })));
    }

    [Fact]
    public void NoProgressRule_SingleMeasurement_IsSatisfied()
    {
        Assert.True(new NoProgressRule().IsSatisfied(Context(new List<Measurement> { Meas(5, 35, 25) })));
    }

    [Fact]
    public void NoProgressRule_MuscleIncreased_IsNotSatisfied()
    {
        var measurements = new List<Measurement> { Meas(3, 35, 25), Meas(15, 37, 25) };

        Assert.False(new NoProgressRule().IsSatisfied(Context(measurements)));
    }

    [Fact]
    public void NoProgressRule_BodyFatDecreased_IsNotSatisfied()
    {
        var measurements = new List<Measurement> { Meas(3, 35, 25), Meas(15, 35, 23) };

        Assert.False(new NoProgressRule().IsSatisfied(Context(measurements)));
    }

    [Fact]
    public void NoProgressRule_NoImprovement_IsSatisfied()
    {
        var measurements = new List<Measurement> { Meas(3, 35, 25), Meas(15, 34, 26) };

        Assert.True(new NoProgressRule().IsSatisfied(Context(measurements)));
    }

    [Fact]
    public void NoProgressRule_MeasurementsOutsideTheRange_AreIgnored()
    {
        var measurements = new List<Measurement>
        {
            new() { UserId = 1, Date = new DateOnly(2026, 10, 10), MuscleKg = 30, BodyFatPercent = 28 },
            new() { UserId = 1, Date = new DateOnly(2026, 10, 25), MuscleKg = 36, BodyFatPercent = 20 }
        };

        Assert.True(new NoProgressRule().IsSatisfied(Context(measurements)));
    }

    [Fact]
    public void GoalsNotMetRule_NoMeasurements_IsSatisfied()
    {
        Assert.True(new GoalsNotMetRule().IsSatisfied(Context()));
    }

    [Fact]
    public void GoalsNotMetRule_MuscleGoalMet_IsNotSatisfied()
    {
        Assert.False(new GoalsNotMetRule().IsSatisfied(Context(new List<Measurement> { Meas(10, 41, 25) })));
    }

    [Fact]
    public void GoalsNotMetRule_BodyFatGoalMet_IsNotSatisfied()
    {
        Assert.False(new GoalsNotMetRule().IsSatisfied(Context(new List<Measurement> { Meas(10, 35, 17) })));
    }

    [Fact]
    public void GoalsNotMetRule_NeitherGoalMet_IsSatisfied()
    {
        Assert.True(new GoalsNotMetRule().IsSatisfied(Context(new List<Measurement> { Meas(10, 35, 25) })));
    }
}
