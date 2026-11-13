using StrongHub.Controllers;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class TrainerControllerTests
{
    private readonly InMemoryRepository<Trainer> _trainers = new();
    private readonly InMemoryRepository<BasicUser> _users = new();
    private readonly SessionContext _session = new();
    private readonly TrainerController _controller;

    public TrainerControllerTests()
    {
        _trainers.Add(new Trainer { Name = "Zoe", Username = "zoe", Type = TrainerType.Gym });
        _trainers.Add(new Trainer { Name = "Ana", Username = "ana", Type = TrainerType.Personal });
        _users.Add(new BasicUser { Name = "Member", Username = "member" });
        _controller = new TrainerController(_trainers, _users, _session);
    }

    private void SignInMember() => _session.SignIn(_users.GetAll()[0]);

    [Fact]
    public void GetTrainerRows_NoFilter_ReturnsAllSortedByName()
    {
        IReadOnlyList<TrainerRow> rows = _controller.GetTrainerRows();

        Assert.Equal(2, rows.Count);
        Assert.Equal("Ana", rows[0].Name);
    }

    [Fact]
    public void GetTrainerRows_FilterByType_ReturnsOnlyThatType()
    {
        IReadOnlyList<TrainerRow> rows = _controller.GetTrainerRows(TrainerType.Gym);

        Assert.Single(rows);
        Assert.Equal("Gym", rows[0].Type);
    }

    [Fact]
    public void AssignTrainer_Member_SavesTrainerId()
    {
        SignInMember();

        _controller.AssignTrainer(1);

        Assert.Equal(1, _users.GetById(1)!.TrainerId);
    }

    [Fact]
    public void AssignTrainer_UnknownTrainer_ThrowsValidationException()
    {
        SignInMember();

        Assert.Throws<ValidationException>(() => _controller.AssignTrainer(99));
    }

    [Fact]
    public void AssignTrainer_AsTrainer_ThrowsValidationException()
    {
        _session.SignIn(_trainers.GetAll()[0]);

        Assert.False(_controller.CanAssign);
        Assert.Throws<ValidationException>(() => _controller.AssignTrainer(1));
    }

    [Fact]
    public void GetAssignedTrainerText_NoTrainer_SaysNoneAssigned()
    {
        SignInMember();

        Assert.Equal("You have no trainer assigned.", _controller.GetAssignedTrainerText());
    }

    [Fact]
    public void GetAssignedTrainerText_WithTrainer_ShowsNameAndType()
    {
        SignInMember();
        _controller.AssignTrainer(2);

        Assert.Equal("Your trainer: Ana (Personal)", _controller.GetAssignedTrainerText());
    }
}
