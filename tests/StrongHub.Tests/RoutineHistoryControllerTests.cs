using StrongHub.Controllers;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class RoutineHistoryControllerTests
{
    private static readonly DateOnly Today = new(2026, 11, 18);

    private readonly InMemoryRoutineRepository _routines = new();
    private readonly InMemoryRepository<Trainer> _trainers = new();
    private readonly SessionContext _session = new();
    private readonly RoutineHistoryController _controller;

    public RoutineHistoryControllerTests()
    {
        _trainers.Add(new Trainer { Name = "Rosaura Chacon", Username = "rosaura" });
        _session.SignIn(new BasicUser { Id = 1, Name = "Ximena Solis", Username = "ximena" });
        _controller = new RoutineHistoryController(_routines, _trainers, _session, () => Today);
    }

    private void AddRoutine(string name, DateOnly start, DateOnly end, int userId = 1, int trainerId = 1,
        List<Exercise>? exercises = null) =>
        _routines.Add(new Routine
        {
            UserId = userId,
            TrainerId = trainerId,
            Name = name,
            StartDate = start,
            EndDate = end,
            Exercises = exercises ?? new List<Exercise>()
        });

    private void AddSevenRoutines()
    {
        for (int number = 1; number <= 7; number++)
        {
            DateOnly start = new DateOnly(2026, 9, 1).AddDays(number * 7);
            AddRoutine($"Routine {number}", start, start.AddDays(28));
        }
    }

    [Fact]
    public void GetLatestRows_ReturnsOnlyTheFiveMostRecentNewestFirst()
    {
        AddSevenRoutines();

        IReadOnlyList<MyRoutineRow> rows = _controller.GetLatestRows();

        Assert.Equal(5, rows.Count);
        Assert.Equal("Routine 7", rows[0].Name);
        Assert.Equal("Routine 3", rows[4].Name);
    }

    [Fact]
    public void GetLatestRows_DoesNotIncludeRoutinesOfOtherMembers()
    {
        AddRoutine("Mine", new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 28));
        AddRoutine("Not mine", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 28), userId: 2);

        MyRoutineRow row = Assert.Single(_controller.GetLatestRows());

        Assert.Equal("Mine", row.Name);
    }

    [Fact]
    public void GetLatestRows_ShowsFinishedActiveAndUpcomingStatus()
    {
        AddRoutine("Finished one", new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 28));
        AddRoutine("Active one", new DateOnly(2026, 11, 10), new DateOnly(2026, 12, 8));
        AddRoutine("Upcoming one", new DateOnly(2026, 11, 25), new DateOnly(2026, 12, 23));

        IReadOnlyList<MyRoutineRow> rows = _controller.GetLatestRows();

        Assert.Equal("Upcoming", rows[0].Status);
        Assert.Equal("Active", rows[1].Status);
        Assert.Equal("Finished", rows[2].Status);
    }

    [Fact]
    public void GetLatestRows_ShowsTheTrainerName()
    {
        AddRoutine("Block", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 28));

        Assert.Equal("Rosaura Chacon", _controller.GetLatestRows()[0].Trainer);
    }

    [Fact]
    public void GetLatestRows_UnknownTrainer_ShowsAFallbackName()
    {
        AddRoutine("Block", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 28), trainerId: 99);

        Assert.Equal("Unknown trainer", _controller.GetLatestRows()[0].Trainer);
    }

    [Fact]
    public void GetLatestRows_DescribesTheExercises()
    {
        var exercises = new List<Exercise>
        {
            new() { Name = "Squat", Sets = 4, Reps = 8, WeightKg = 60 },
            new() { Name = "Plank", Sets = 3, Reps = 1, WeightKg = 0 }
        };
        AddRoutine("Block", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 28), exercises: exercises);

        Assert.Equal("Squat 4x8 @ 60 kg; Plank 3x1 @ 0 kg", _controller.GetLatestRows()[0].Exercises);
    }

    [Fact]
    public void GetLatestRows_SignedInAsTrainer_ReturnsNothing()
    {
        AddRoutine("Block", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 28));
        _session.SignIn(new Trainer { Id = 1, Name = "Rosaura Chacon", Username = "rosaura" });

        Assert.Empty(_controller.GetLatestRows());
    }

    [Fact]
    public void GetSummary_WithoutRoutines_SaysSo()
    {
        Assert.Equal("You have no routines yet.", _controller.GetSummary());
    }

    [Fact]
    public void GetSummary_FewRoutines_CountsThem()
    {
        AddRoutine("A", new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 28));
        AddRoutine("B", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 28));

        Assert.Equal("2 routine(s), newest first.", _controller.GetSummary());
    }

    [Fact]
    public void GetSummary_MoreThanFive_ExplainsTheLimit()
    {
        AddSevenRoutines();

        Assert.Equal("Latest 5 of 7 routines, newest first.", _controller.GetSummary());
    }
}
