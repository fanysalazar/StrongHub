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
        var gymClasses = new JsonRepository<GymClass>(Path.Combine(dataFolder, "gymclasses.json"));
        var routines = new RoutineRepository(Path.Combine(dataFolder, "routines.json"));
        var routineChanges = new JsonRepository<RoutineChange>(Path.Combine(dataFolder, "routinechanges.json"));

        var session = new SessionContext();
        var authController = new AuthController(users, trainers, session);
        var registerController = new RegisterController(users, trainers);
        var menuController = new MainMenuController(session);
        var trainerController = new TrainerController(trainers, users, session);
        var catalogController = new ClassCatalogController(gymClasses);
        var routineController = new RoutineController(routines, users, session, routineChanges);
        var changesController = new RoutineChangesController(routineChanges, routines, session);

        // Sprint 3: the notification center will subscribe to routineController.RoutineCreated here.

        while (true)
        {
            using var loginForm = new LoginForm(authController, registerController);
            if (loginForm.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using var menuForm = new MainMenuForm(
                menuController, trainerController, catalogController, routineController, changesController);
            if (menuForm.ShowDialog() != DialogResult.Retry)
            {
                return;
            }

            session.SignOut();
        }
    }
}
