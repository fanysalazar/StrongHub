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
    private readonly RoutineController _controller;

    public RoutineControllerTests()
    {
        _users.Add(new BasicUser { Name = "Zaira Beltran", Username = "zaira", TrainerId = 7 });
        _users.Add(new BasicUser { Name = "Anselmo Rueda", Username = "anselmo", TrainerId = 3 });
        _session.SignIn(new Trainer { Id = 7, Name = "Marco Villalta", Username = "marcov" });
        _controller = new RoutineController(_routines, _users, _session);
    }

    private static List<Exercise> OneExercise() =>
        new() { new Exercise { Name = "Squat", Sets = 4, Reps = 8, WeightKg = 60 } };

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
}
