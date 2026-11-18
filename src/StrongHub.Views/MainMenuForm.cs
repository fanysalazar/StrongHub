namespace StrongHub.Views;

/// <summary>Main menu; shows different options for members and trainers.</summary>
public class MainMenuForm : Form
{
    private readonly AppControllers _app;
    private readonly Label _notificationLabel = new() { AutoSize = true, ForeColor = Color.DimGray };

    public MainMenuForm(AppControllers app)
    {
        _app = app;
        BuildLayout();
    }

    private void BuildLayout()
    {
        IReadOnlyList<string> options = _app.Menu.GetOptions();

        Text = "StrongHub - Main menu";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        ClientSize = new Size(360, 145 + options.Count * 45);

        Controls.Add(new Label
        {
            Text = _app.Menu.GetWelcomeMessage(),
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 15)
        });

        _notificationLabel.Location = new Point(20, 45);
        Controls.Add(_notificationLabel);
        RefreshNotificationLabel();

        int top = 75;
        foreach (string option in options)
        {
            var button = new Button { Text = option, Location = new Point(20, top), Size = new Size(320, 36) };
            button.Click += (_, _) => OpenOption(option);
            Controls.Add(button);
            top += 45;
        }

        var logoutButton = new Button { Text = "Log out", Location = new Point(20, top + 10), Size = new Size(320, 36) };
        logoutButton.Click += (_, _) => DialogResult = DialogResult.Retry;
        Controls.Add(logoutButton);
    }

    private void RefreshNotificationLabel()
    {
        int unseen = _app.Timeline.GetUnseenCount();
        _notificationLabel.Text = unseen == 0
            ? "No new notifications"
            : $"New notifications: {unseen} (open Timeline)";
    }

    private void ShowDialogOf(Form form)
    {
        using (form)
        {
            form.ShowDialog(this);
        }
    }

    private void OpenOption(string option)
    {
        switch (option)
        {
            case "My history":
                ShowDialogOf(new MyHistoryForm(_app));
                break;
            case "Trainers":
                ShowDialogOf(new TrainersForm(_app.Trainers));
                break;
            case "Gym classes":
                ShowDialogOf(new GymClassesForm(_app.Catalog));
                break;
            case "Manage routines":
                ShowDialogOf(new RoutineListForm(_app.Routines));
                break;
            case "Load increases":
                ShowDialogOf(new RoutineChangesForm(_app.RoutineChanges));
                break;
            case "Register visit":
                ShowDialogOf(new VisitForm(_app.Visits));
                break;
            case "My badges":
                ShowDialogOf(new BadgesForm(_app.Badges));
                break;
            case "Shame badges":
                ShowDialogOf(new ShameBadgesForm(_app.ShameBadges));
                break;
            case "Timeline":
                ShowDialogOf(new TimelineForm(_app.Timeline));
                break;
            default:
                MessageBox.Show($"'{option}' will be available in a later sprint.", "StrongHub");
                break;
        }

        RefreshNotificationLabel();
    }
}
