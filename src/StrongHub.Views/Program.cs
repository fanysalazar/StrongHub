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
        var visits = new VisitRepository(Path.Combine(dataFolder, "visits.json"));
        var measurements = new JsonRepository<Measurement>(Path.Combine(dataFolder, "measurements.json"));
        var userBadges = new JsonRepository<UserBadge>(Path.Combine(dataFolder, "badges.json"));
        var notifications = new JsonRepository<Notification>(Path.Combine(dataFolder, "notifications.json"));

        var session = new SessionContext();
        var authController = new AuthController(users, trainers, session);
        var registerController = new RegisterController(users, trainers);
        var menuController = new MainMenuController(session);
        var trainerController = new TrainerController(trainers, users, session);
        var catalogController = new ClassCatalogController(gymClasses);
        var routineController = new RoutineController(routines, users, session, routineChanges);
        var changesController = new RoutineChangesController(routineChanges, routines, session);
        var visitController = new VisitController(visits, gymClasses, session);

        var contextFactory = new BadgeContextFactory(measurements, visits);
        var achievementEvaluator = new BadgeEvaluator(new IBadgeRule[]
        {
            new MuscleGoalRule(),
            new BodyFatGoalRule(),
            new VisitCountRule("regular-visitor", "Regular visitor", 6, 30),
            new VisitCountRule("gym-devotee", "Gym devotee", 12, 30)
        });
        var badgeController = new BadgeController(userBadges, achievementEvaluator, contextFactory, session);

        var shameEvaluator = new BadgeEvaluator(new IBadgeRule[]
        {
            new LowestVisitsRule(),
            new NoProgressRule(),
            new GoalsNotMetRule()
        });
        var shameController = new ShameBadgeController(
            users, userBadges, visits, shameEvaluator, contextFactory, session);

        // Observer pattern: the notification center listens to the controllers and writes the timeline.
        var unseenTracker = new UnseenNotificationTracker();
        var notificationCenter = new NotificationCenter(notifications);
        notificationCenter.Subscribe(unseenTracker);
        notificationCenter.ListenTo(routineController);
        notificationCenter.ListenTo(visitController);
        notificationCenter.ListenTo(badgeController);
        // Next sprint: shameController.ShameBadgeAssigned (1.5.3) and the trainer alerts (1.5.2).

        var timelineController = new TimelineController(notifications, unseenTracker, session);

        var app = new AppControllers(
            menuController, trainerController, catalogController, routineController,
            changesController, visitController, badgeController, shameController, timelineController);

        while (true)
        {
            using var loginForm = new LoginForm(authController, registerController);
            if (loginForm.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using var menuForm = new MainMenuForm(app);
            if (menuForm.ShowDialog() != DialogResult.Retry)
            {
                return;
            }

            session.SignOut();
        }
    }
}
