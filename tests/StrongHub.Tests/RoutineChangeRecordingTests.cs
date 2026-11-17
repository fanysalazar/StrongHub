using StrongHub.Controllers;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class RoutineChangeRecordingTests
{
    private static readonly DateOnly Start = new(2026, 11, 16);
    private static readonly DateOnly End = new(2026, 12, 14);

    private readonly InMemoryRoutineRepository _routines = new();
    private readonly InMemoryRepository<BasicUser> _users = new();
    private readonly InMemoryRepository<RoutineChange> _changes = new();
    private readonly SessionContext _session = new();
    private readonly RoutineController _controller;

    public RoutineChangeRecordingTests()
    {
        _users.Add(new BasicUser { Name = "Zaira Beltran", Username = "zaira" });
        _session.SignIn(new Trainer { Id = 7, Name = "Marco Villalta", Username = "marcov" });
        _controller = new RoutineController(_routines, _users, _session, _changes);
    }

    private Routine SeedRoutine()
    {
        var routine = new Routine
        {
            UserId = 1,
            TrainerId = 7,
            Name = "Seed block",
            StartDate = Start,
            EndDate = End,
            Exercises = new List<Exercise> { new() { Name = "Squat", Sets = 4, Reps = 8, WeightKg = 60 } }
        };
        _routines.Add(routine);
        return routine;
    }

    [Fact]
    public void Update_ChangedWeight_RecordsOldValueNewValueAndToday()
    {
        Routine seeded = SeedRoutine();
        var edited = new List<Exercise> { new() { Name = "Squat", Sets = 4, Reps = 8, WeightKg = 65 } };

        _controller.Update(seeded.Id, "Seed block", Start, End, edited);

        RoutineChange change = Assert.Single(_changes.GetAll());
        Assert.Equal(60, change.OldWeightKg);
        Assert.Equal(65, change.NewWeightKg);
        Assert.Equal(seeded.Id, change.RoutineId);
        Assert.Equal(1, change.UserId);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Today), change.Date);
    }

    [Fact]
    public void Update_SameWeight_RecordsNothing()
    {
        Routine seeded = SeedRoutine();
        var edited = new List<Exercise> { new() { Name = "Squat", Sets = 5, Reps = 5, WeightKg = 60 } };

        _controller.Update(seeded.Id, "Renamed block", Start, End, edited);

        Assert.Empty(_changes.GetAll());
    }

    [Fact]
    public void Update_InvalidData_RecordsNothing()
    {
        Routine seeded = SeedRoutine();
        var edited = new List<Exercise> { new() { Name = "Squat", Sets = 4, Reps = 8, WeightKg = 70 } };

        Assert.Throws<ValidationException>(() =>
            _controller.Update(seeded.Id, "Seed block", End, Start, edited));

        Assert.Empty(_changes.GetAll());
    }
}
