namespace StrongHub.Views;

/// <summary>Main menu; shows different options for members and trainers.</summary>
public class MainMenuForm : Form
{
    private readonly AppControllers _app;

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
        ClientSize = new Size(360, 130 + options.Count * 45);

        Controls.Add(new Label
        {
            Text = _app.Menu.GetWelcomeMessage(),
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 15)
        });

        int top = 60;
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
            default:
                MessageBox.Show($"'{option}' will be available in a later sprint.", "StrongHub");
                break;
        }
    }
}

