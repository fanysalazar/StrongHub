using StrongHub.Controllers;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class AuthControllerTests
{
    private readonly InMemoryRepository<BasicUser> _users = new();
    private readonly InMemoryRepository<Trainer> _trainers = new();
    private readonly SessionContext _session = new();
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _users.Add(new BasicUser { Name = "Ana", Username = "user1" });
        _trainers.Add(new Trainer { Name = "Coach", Username = "trainer1" });
        _controller = new AuthController(_users, _trainers, _session);
    }

    [Fact]
    public void Login_BasicUser_SignsInAsNonTrainer()
    {
        _controller.Login("user1");

        Assert.NotNull(_session.CurrentUser);
        Assert.False(_session.IsTrainer);
    }

    [Fact]
    public void Login_Trainer_SignsInAsTrainer()
    {
        _controller.Login("TRAINER1");

        Assert.True(_session.IsTrainer);
    }

    [Fact]
    public void Login_UnknownUser_ThrowsUserNotFoundException()
    {
        Assert.Throws<UserNotFoundException>(() => _controller.Login("nobody"));
    }

    [Fact]
    public void Login_EmptyUsername_ThrowsValidationException()
    {
        Assert.Throws<ValidationException>(() => _controller.Login("   "));
    }
}
