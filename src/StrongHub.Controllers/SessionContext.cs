using StrongHub.Models;

namespace StrongHub.Controllers;

/// <summary>Holds the user who is currently logged in.</summary>
public class SessionContext
{
    public User? CurrentUser { get; private set; }

    public bool IsTrainer => CurrentUser is Trainer;

    public void SignIn(User user) => CurrentUser = user;

    public void SignOut() => CurrentUser = null;
}
