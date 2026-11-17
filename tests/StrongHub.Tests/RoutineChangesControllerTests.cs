using StrongHub.Controllers;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class RoutineChangesControllerTests
{
    private readonly InMemoryRepository<RoutineChange> _changes = new();
    private readonly InMemoryRoutineRepository _routines = new();
    private readonly InMemoryRepository<BasicUser> _users = new();
    private readonly SessionContext _session = new();
    private readonly RoutineChangesController _controller;

    public RoutineChangesControllerTests()
    {
        _users.Add(new BasicUser { Name = "Zaira Beltran", Username = "zaira" });
        _users.Add(new BasicUser { Name = "Anselmo Rueda", Username = "anselmo" });
        _routines.Add(new Routine { UserId = 1, Name = "Block A" });

        _changes.Add(Change(userId: 1, day: new DateOnly(2026, 11, 1), oldKg: 60, newKg: 65));
        _changes.Add(Change(userId: 1, day: new DateOnly(2026, 11, 10), oldKg: 65, newKg: 70));
        _changes.Add(Change(userId: 2, day: new DateOnly(2026, 11, 5), oldKg: 30, newKg: 40));

        _controller = new RoutineChangesController(_changes, _routines, _session);
    }

    private static RoutineChange Change(int userId, DateOnly day, double oldKg, double newKg) =>
        new() { RoutineId = 1, UserId = userId, ExerciseName = "Squat", Date = day, OldWeightKg = oldKg, NewWeightKg = newKg };

    [Fact]
    public void GetRows_Member_ReturnsOnlyOwnChangesNewestFirst()
    {
        _session.SignIn(_users.GetAll()[0]);

        IReadOnlyList<RoutineChangeRow> rows = _controller.GetRows();

        Assert.Equal(2, rows.Count);
        Assert.Equal("2026-11-10", rows[0].Date);
        Assert.Equal("+5", rows[0].Change);
        Assert.Equal("Block A", rows[0].Routine);
    }

    [Fact]
    public void GetRows_Trainer_ReturnsNothing()
    {
        _session.SignIn(new Trainer { Name = "Marco Villalta", Username = "marcov" });

        Assert.Empty(_controller.GetRows());
    }

    [Fact]
    public void GetSummary_Member_CountsIncreasesAndTotalKg()
    {
        _session.SignIn(_users.GetAll()[0]);

        Assert.Equal("2 load increase(s) recorded, +10 kg in total", _controller.GetSummary());
    }

    [Fact]
    public void GetSummary_MemberWithoutChanges_SaysSo()
    {
        _users.Add(new BasicUser { Name = "Nueva Persona", Username = "nueva" });
        _session.SignIn(_users.GetAll()[2]);

        Assert.Equal("No load increases recorded yet.", _controller.GetSummary());
    }
}
