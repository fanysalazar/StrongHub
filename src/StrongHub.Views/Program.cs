using StrongHub.Controllers;
using StrongHub.Data;
using StrongHub.Models;

namespace StrongHub.Views;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            Run();
        }
        catch (DataAccessException ex)
        {
            MessageBox.Show(ex.Message, "StrongHub - Data error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void Run()
    {
        string dataFolder = Path.Combine(AppContext.BaseDirectory, "data");
        var users = new JsonRepository<BasicUser>(Path.Combine(dataFolder, "users.json"));
        var trainers = new JsonRepository<Trainer>(Path.Combine(dataFolder, "trainers.json"));

        var session = new SessionContext();
        var authController = new AuthController(users, trainers, session);
        var registerController = new RegisterController(users, trainers);
        var menuController = new MainMenuController(session);

        while (true)
        {
            using var loginForm = new LoginForm(authController, registerController);
            if (loginForm.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using var menuForm = new MainMenuForm(menuController);
            if (menuForm.ShowDialog() != DialogResult.Retry)
            {
                return;
            }

            session.SignOut();
        }
    }
}
