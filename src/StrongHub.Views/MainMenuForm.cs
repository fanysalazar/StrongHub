using StrongHub.Controllers;

namespace StrongHub.Views;

/// <summary>Main menu; shows different options for members and trainers.</summary>
public class MainMenuForm : Form
{
    private readonly MainMenuController _controller;

    public MainMenuForm(MainMenuController controller)
    {
        _controller = controller;
        BuildLayout();
    }

    private void BuildLayout()
    {
        IReadOnlyList<string> options = _controller.GetOptions();

        Text = "StrongHub - Main menu";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        ClientSize = new Size(360, 130 + options.Count * 45);

        Controls.Add(new Label
        {
            Text = _controller.GetWelcomeMessage(),
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 15)
        });

        int top = 60;
        foreach (string option in options)
        {
            var button = new Button { Text = option, Location = new Point(20, top), Size = new Size(320, 36) };
            button.Click += (_, _) =>
                MessageBox.Show($"'{option}' will be available in a later sprint.", "StrongHub");
            Controls.Add(button);
            top += 45;
        }

        var logoutButton = new Button { Text = "Log out", Location = new Point(20, top + 10), Size = new Size(320, 36) };
        logoutButton.Click += (_, _) => DialogResult = DialogResult.Retry;
        Controls.Add(logoutButton);
    }
}
