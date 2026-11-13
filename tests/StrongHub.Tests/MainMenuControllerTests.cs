using StrongHub.Controllers;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class MainMenuControllerTests
{
    [Fact]
    public void GetOptions_BasicUser_ShowsMemberOptions()
    {
        var session = new SessionContext();
        session.SignIn(new BasicUser { Name = "Ana", Username = "ana" });

        IReadOnlyList<string> options = new MainMenuController(session).GetOptions();

        Assert.Contains("Statistics", options);
        Assert.Contains("Trainers", options);
        Assert.DoesNotContain("Manage routines", options);
    }

    [Fact]
    public void GetOptions_Trainer_ShowsTrainerOptions()
    {
        var session = new SessionContext();
        session.SignIn(new Trainer { Name = "Coach", Username = "coach" });

        IReadOnlyList<string> options = new MainMenuController(session).GetOptions();

        Assert.Contains("Manage routines", options);
        Assert.Contains("Trainers", options);
        Assert.DoesNotContain("Statistics", options);
    }
}
