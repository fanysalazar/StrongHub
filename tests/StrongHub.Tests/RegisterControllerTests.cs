using StrongHub.Controllers;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class RegisterControllerTests
{
    private readonly InMemoryRepository<BasicUser> _users = new();
    private readonly InMemoryRepository<Trainer> _trainers = new();
    private readonly RegisterController _controller;

    public RegisterControllerTests()
    {
        _controller = new RegisterController(_users, _trainers);
    }

    [Fact]
    public void Register_ValidData_SavesUserWithBmi()
    {
        BasicUser user = _controller.Register("Ana Mora", "ana", "30", "70", "1.75");

        Assert.Equal(22.86, user.Bmi);
        Assert.Single(_users.GetAll());
    }

    [Fact]
    public void Register_CommaDecimals_AreAccepted()
    {
        BasicUser user = _controller.Register("Ana Mora", "ana", "30", "70,5", "1,75");

        Assert.Equal(70.5, user.WeightKg);
    }

    [Fact]
    public void Register_DuplicateUsername_ThrowsValidationException()
    {
        _controller.Register("Ana Mora", "ana", "30", "70", "1.75");

        Assert.Throws<ValidationException>(() =>
            _controller.Register("Otra Ana", "ANA", "25", "60", "1.65"));
    }

    [Fact]
    public void Register_UsernameUsedByTrainer_ThrowsValidationException()
    {
        _trainers.Add(new Trainer { Name = "Coach", Username = "coach" });

        Assert.Throws<ValidationException>(() =>
            _controller.Register("Ana Mora", "coach", "30", "70", "1.75"));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("5")]
    [InlineData("150")]
    public void Register_InvalidAge_ThrowsValidationException(string age)
    {
        Assert.Throws<ValidationException>(() =>
            _controller.Register("Ana Mora", "ana", age, "70", "1.75"));
    }

    [Fact]
    public void Register_EmptyName_ThrowsValidationException()
    {
        Assert.Throws<ValidationException>(() =>
            _controller.Register("  ", "ana", "30", "70", "1.75"));
    }

    [Fact]
    public void PreviewBmi_IncompleteInput_ReturnsNull()
    {
        Assert.Null(_controller.PreviewBmi("70", ""));
    }
}
