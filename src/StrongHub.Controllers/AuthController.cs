using StrongHub.Data;
using StrongHub.Models;

namespace StrongHub.Controllers;

/// <summary>Handles the login of basic users and trainers.</summary>
public class AuthController
{
    private readonly IRepository<BasicUser> _users;
    private readonly IRepository<Trainer> _trainers;
    private readonly SessionContext _session;

    public AuthController(IRepository<BasicUser> users, IRepository<Trainer> trainers, SessionContext session)
    {
        _users = users;
        _trainers = trainers;
        _session = session;
    }

    /// <summary>Signs in by username. Throws if the text is empty or the user does not exist.</summary>
    public void Login(string username)
    {
        string normalized = username.Trim();
        if (normalized.Length == 0)
        {
            throw new ValidationException("Username is required.");
        }

        User? user = FindUser(normalized);
        if (user is null)
        {
            throw new UserNotFoundException(normalized);
        }

        _session.SignIn(user);
    }

    private User? FindUser(string username)
    {
        User? basicUser = _users.GetAll().FirstOrDefault(user => SameUsername(user.Username, username));
        return basicUser ?? _trainers.GetAll().FirstOrDefault(trainer => SameUsername(trainer.Username, username));
    }

    private static bool SameUsername(string first, string second) =>
        string.Equals(first, second, StringComparison.OrdinalIgnoreCase);
}
