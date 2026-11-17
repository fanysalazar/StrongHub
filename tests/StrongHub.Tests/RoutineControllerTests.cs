using StrongHub.Controllers;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class RoutineControllerTests
{
    private static readonly DateOnly Start = new(2026, 11, 16);
    private static readonly DateOnly End = new(2026, 12, 14);

    private readonly InMemoryRoutineRepository _routines = new();
    private readonly InMemoryRepository<BasicUser> _users = new();
    private readonly SessionContext _session = new();
    private readonly InMemoryRepository<RoutineChange> _changes = new();
    private readonly RoutineController _controller;

    public RoutineControllerTests()
    {
        _users.Add(new BasicUser { Name = "Zaira Beltran", Username = "zaira", TrainerId = 7 });
        _users.Add(new BasicUser { Name = "Anselmo Rueda", Username = "anselmo", TrainerId = 3 });
        _session.SignIn(new Trainer { Id = 7, Name = "Marco Villalta", Username = "marcov" });
        _controller = new RoutineController(_routines, _users, _session, _changes);
    }

    private static List<Exercise> OneExercise() =>
        new() { new Exercise { Name = "Squat", Sets = 4, Reps = 8, WeightKg = 60 } };

    private Routine SeedRoutine(int memberId = 1, int trainerId = 7)
    {
        var routine = new Routine
        {
            UserId = memberId,
            TrainerId = trainerId,
            Name = "Seed block",
            StartDate = Start,
            EndDate = End,
            Exercises = OneExercise()
        };
        _routines.Add(routine);
        return routine;
    }

    [Fact]
    public void Create_ValidData_SavesRoutineAndRaisesEvent()
    {
        RoutineCreatedEventArgs? raised = null;
        _controller.RoutineCreated += (_, args) => raised = args;

        Routine saved = _controller.Create(1, "  Strength block  ", Start, End, OneExercise());

        Assert.Equal("Strength block", saved.Name);
        Assert.Equal(7, saved.TrainerId);
        Assert.Single(_routines.GetAll());
        Assert.NotNull(raised);
        Assert.Equal("Zaira Beltran", raised!.MemberName);
        Assert.Equal("Marco Villalta", raised.TrainerName);
        Assert.Equal(saved.Id, raised.Routine.Id);
    }

    [Fact]
    public void Create_EndBeforeStart_ThrowsValidationException()
    {
        Assert.Throws<ValidationException>(() =>
            _controller.Create(1, "Strength block", End, Start, OneExercise()));
    }

    [Fact]
    public void Create_NoExercises_ThrowsValidationException()
    {
        Assert.Throws<ValidationException>(() =>
            _controller.Create(1, "Strength block", Start, End, new List<Exercise>()));
    }

    [Fact]
    public void Create_ShortName_ThrowsValidationException()
    {
        Assert.Throws<ValidationException>(() =>
            _controller.Create(1, "ab", Start, End, OneExercise()));
    }

    [Fact]
    public void Create_UnknownMember_ThrowsValidationException()
    {
        Assert.Throws<ValidationException>(() =>
            _controller.Create(99, "Strength block", Start, End, OneExercise()));
    }

    [Fact]
    public void Create_SignedInAsMember_ThrowsValidationException()
    {
        _session.SignIn(_users.GetAll()[0]);

        Assert.False(_controller.CanCreate);
        Assert.Throws<ValidationException>(() =>
            _controller.Create(1, "Strength block", Start, End, OneExercise()));
    }

    [Fact]
    public void ParseExercise_CommaWeight_IsAccepted()
    {
        Exercise exercise = _controller.ParseExercise(" Bench press ", "3", "10", "42,5");

        Assert.Equal("Bench press", exercise.Name);
        Assert.Equal(42.5, exercise.WeightKg);
    }

    [Theory]
    [InlineData("", "3", "10", "20")]
    [InlineData("Squat", "0", "10", "20")]
    [InlineData("Squat", "3", "abc", "20")]
    [InlineData("Squat", "3", "10", "-5")]
    public void ParseExercise_InvalidInput_ThrowsValidationException(
        string name, string sets, string reps, string weight)
    {
        Assert.Throws<ValidationException>(() => _controller.ParseExercise(name, sets, reps, weight));
    }

    [Fact]
    public void GetMemberOptions_PutsOwnMembersFirst()
    {
        IReadOnlyList<MemberOption> options = _controller.GetMemberOptions();

        Assert.Equal(2, options.Count);
        Assert.Equal(1, options[0].Id);
        Assert.Contains("your member", options[0].Label);
    }

    [Fact]
    public void CountRoutinesOf_CountsOnlyThatMember()
    {
        _controller.Create(1, "Strength block", Start, End, OneExercise());

        Assert.Equal(1, _controller.CountRoutinesOf(1));
        Assert.Equal(0, _controller.CountRoutinesOf(2));
    }

    [Fact]
    public void Update_ValidData_ChangesRoutineAndKeepsMember()
    {
        Routine seeded = SeedRoutine();
        var newExercises = new List<Exercise> { new() { Name = "Lunge", Sets = 3, Reps = 12, WeightKg = 20 } };

        Routine updated = _controller.Update(seeded.Id, "  New name ", Start.AddDays(1), End, newExercises);

        Assert.Equal("New name", updated.Name);
        Assert.Equal(Start.AddDays(1), updated.StartDate);
        Assert.Equal("Lunge", _routines.GetById(seeded.Id)!.Exercises[0].Name);
        Assert.Equal(1, updated.UserId);
    }

    [Fact]
    public void Update_UnknownRoutine_ThrowsRoutineNotFoundException()
    {
        Assert.Throws<RoutineNotFoundException>(() =>
            _controller.Update(99, "New name", Start, End, OneExercise()));
    }

    [Fact]
    public void Update_EndBeforeStart_ThrowsAndKeepsOriginalData()
    {
        Routine seeded = SeedRoutine();

        Assert.Throws<ValidationException>(() =>
            _controller.Update(seeded.Id, "Changed", End, Start, OneExercise()));

        Assert.Equal("Seed block", _routines.GetById(seeded.Id)!.Name);
    }

    [Fact]
    public void Update_SignedInAsMember_ThrowsValidationException()
    {
        Routine seeded = SeedRoutine();
        _session.SignIn(_users.GetAll()[0]);

        Assert.Throws<ValidationException>(() =>
            _controller.Update(seeded.Id, "Changed", Start, End, OneExercise()));
    }

    [Fact]
    public void Delete_ExistingRoutine_RemovesIt()
    {
        Routine seeded = SeedRoutine();

        _controller.Delete(seeded.Id);

        Assert.Empty(_routines.GetAll());
    }

    [Fact]
    public void Delete_UnknownRoutine_ThrowsRoutineNotFoundException()
    {
        Assert.Throws<RoutineNotFoundException>(() => _controller.Delete(99));
    }

    [Fact]
    public void Delete_SignedInAsMember_ThrowsAndKeepsRoutine()
    {
        Routine seeded = SeedRoutine();
        _session.SignIn(_users.GetAll()[0]);

        Assert.Throws<ValidationException>(() => _controller.Delete(seeded.Id));
        Assert.Single(_routines.GetAll());
    }

    [Fact]
    public void GetRoutineRows_FilterByMember_ReturnsOnlyThatMember()
    {
        SeedRoutine(memberId: 1);
        SeedRoutine(memberId: 2);

        IReadOnlyList<RoutineRow> rows = _controller.GetRoutineRows(memberId: 2);

        Assert.Single(rows);
        Assert.Equal("Anselmo Rueda", rows[0].Member);
    }

    [Fact]
    public void GetRoutineRows_OnlyMine_ReturnsRoutinesOfTheSignedInTrainer()
    {
        SeedRoutine(memberId: 1, trainerId: 7);
        SeedRoutine(memberId: 2, trainerId: 3);

        IReadOnlyList<RoutineRow> rows = _controller.GetRoutineRows(onlyMine: true);

        Assert.Single(rows);
        Assert.Equal("Zaira Beltran", rows[0].Member);
    }

    [Fact]
    public void GetRoutineForEdit_ReturnsIndependentCopy()
    {
        Routine seeded = SeedRoutine();

        Routine copy = _controller.GetRoutineForEdit(seeded.Id);
        copy.Name = "Changed in the form";
        copy.Exercises[0].Name = "Changed exercise";

        Assert.Equal("Seed block", _routines.GetById(seeded.Id)!.Name);
        Assert.Equal("Squat", _routines.GetById(seeded.Id)!.Exercises[0].Name);
    }

    [Fact]
    public void GetRoutineForEdit_UnknownRoutine_ThrowsRoutineNotFoundException()
    {
        Assert.Throws<RoutineNotFoundException>(() => _controller.GetRoutineForEdit(99));
    }
}

