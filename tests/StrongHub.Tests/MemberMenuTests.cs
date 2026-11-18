using StrongHub.Controllers;
using StrongHub.Models;
using Xunit;

namespace StrongHub.Tests;

public class MemberMenuTests
{
    [Fact]
    public void GetOptions_BasicUser_IncludesTheHistoryOption()
    {
        var session = new SessionContext();
        session.SignIn(new BasicUser { Name = "Zoraida Mejias", Username = "zoraida" });

        Assert.Contains("My history", new MainMenuController(session).GetOptions());
    }

    [Fact]
    public void GetOptions_Trainer_DoesNotIncludeTheHistoryOption()
    {
        var session = new SessionContext();
        session.SignIn(new Trainer { Name = "Rosaura Chacon", Username = "rosaura" });

        Assert.DoesNotContain("My history", new MainMenuController(session).GetOptions());
    }
}
