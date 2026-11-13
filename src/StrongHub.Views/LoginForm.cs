using StrongHub.Controllers;
using StrongHub.Models;

namespace StrongHub.Views;

/// <summary>Login screen with a shortcut to the registration form.</summary>
public class LoginForm : Form
{
    private readonly AuthController _authController;
    private readonly RegisterController _registerController;
    private readonly TextBox _usernameBox = new() { Width = 260 };

    public LoginForm(AuthController authController, RegisterController registerController)
    {
        _authController = authController;
        _registerController = registerController;
        BuildLayout();
    }

    private void BuildLayout()
    {
        Text = "StrongHub - Login";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        ClientSize = new Size(300, 190);

        Controls.Add(new Label
        {
            Text = "StrongHub",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 15)
        });
        Controls.Add(new Label { Text = "Username", AutoSize = true, Location = new Point(20, 60) });

        _usernameBox.Location = new Point(20, 80);
        Controls.Add(_usernameBox);

        var loginButton = new Button { Text = "Login", Location = new Point(20, 120), Size = new Size(125, 34) };
        var registerButton = new Button { Text = "Register", Location = new Point(155, 120), Size = new Size(125, 34) };
        loginButton.Click += OnLoginClick;
        registerButton.Click += OnRegisterClick;
        AcceptButton = loginButton;
        Controls.Add(loginButton);
        Controls.Add(registerButton);
    }

    private void OnLoginClick(object? sender, EventArgs e)
    {
        try
        {
            _authController.Login(_usernameBox.Text);
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex) when (ex is UserNotFoundException or ValidationException)
        {
            MessageBox.Show(ex.Message, "Login error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void OnRegisterClick(object? sender, EventArgs e)
    {
        using var registerForm = new RegisterForm(_registerController);
        registerForm.ShowDialog(this);

        if (registerForm.RegisteredUsername is not null)
        {
            _usernameBox.Text = registerForm.RegisteredUsername;
        }
    }
}
