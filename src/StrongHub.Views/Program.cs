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
            string dataFolder = Path.Combine(AppContext.BaseDirectory, "data");
            var users = new JsonRepository<BasicUser>(Path.Combine(dataFolder, "users.json"));
            var trainers = new JsonRepository<Trainer>(Path.Combine(dataFolder, "trainers.json"));

            Application.Run(new RegisterForm(new RegisterController(users, trainers)));
        }
        catch (DataAccessException ex)
        {
            MessageBox.Show(ex.Message, "StrongHub - Data error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
