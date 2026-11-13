namespace StrongHub.Controllers;

/// <summary>Decides which menu options each role can see.</summary>
public class MainMenuController
{
    private static readonly string[] BasicUserOptions =
        { "My routines", "Gym classes", "Trainers", "Register visit", "My badges", "Timeline", "Statistics" };

    private static readonly string[] TrainerOptions =
        { "Manage routines", "Trainers", "Timeline" };

    private readonly SessionContext _session;

    public MainMenuController(SessionContext session) => _session = session;

    public IReadOnlyList<string> GetOptions() => _session.IsTrainer ? TrainerOptions : BasicUserOptions;

    public string GetWelcomeMessage()
    {
        string role = _session.IsTrainer ? "Trainer" : "Member";
        return $"Welcome, {_session.CurrentUser?.Name} ({role})";
    }
}
