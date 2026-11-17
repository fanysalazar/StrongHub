using StrongHub.Controllers;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class RoutineChangeDetectorTests
{
    private static readonly DateOnly Day = new(2026, 11, 20);

    private static Exercise Lift(string name, double weight) =>
        new() { Name = name, Sets = 3, Reps = 8, WeightKg = weight };

    [Fact]
    public void Detect_WeightChanged_ReturnsOldAndNewValue()
    {
        var before = new List<Exercise> { Lift("Squat", 60) };
        var after = new List<Exercise> { Lift("Squat", 65) };

        List<RoutineChange> changes = RoutineChangeDetector.Detect(before, after, 4, 9, Day);

        RoutineChange change = Assert.Single(changes);
        Assert.Equal(60, change.OldWeightKg);
        Assert.Equal(65, change.NewWeightKg);
        Assert.Equal(4, change.RoutineId);
        Assert.Equal(9, change.UserId);
        Assert.Equal(Day, change.Date);
    }

    [Fact]
    public void Detect_SameWeight_ReturnsNothing()
    {
        var before = new List<Exercise> { Lift("Squat", 60) };
        var after = new List<Exercise> { Lift("Squat", 60) };

        Assert.Empty(RoutineChangeDetector.Detect(before, after, 1, 1, Day));
    }

    [Fact]
    public void Detect_NewOrRemovedExercise_IsNotAWeightChange()
    {
        var before = new List<Exercise> { Lift("Squat", 60), Lift("Plank", 0) };
        var after = new List<Exercise> { Lift("Squat", 60), Lift("Lunge", 20) };

        Assert.Empty(RoutineChangeDetector.Detect(before, after, 1, 1, Day));
    }

    [Fact]
    public void Detect_NameComparisonIgnoresCase()
    {
        var before = new List<Exercise> { Lift("Bench press", 40) };
        var after = new List<Exercise> { Lift("BENCH PRESS", 45) };

        Assert.Single(RoutineChangeDetector.Detect(before, after, 1, 1, Day));
    }
}
