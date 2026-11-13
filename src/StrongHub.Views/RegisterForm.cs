using StrongHub.Controllers;
using StrongHub.Models;

namespace StrongHub.Views;

/// <summary>Form to register a new basic user.</summary>
public class RegisterForm : Form
{
    private readonly RegisterController _controller;
    private readonly TextBox _nameBox = new();
    private readonly TextBox _usernameBox = new();
    private readonly TextBox _ageBox = new();
    private readonly TextBox _weightBox = new();
    private readonly TextBox _heightBox = new();
    private readonly Label _bmiLabel = new() { Text = "BMI: -", AutoSize = true };

    public string? RegisteredUsername { get; private set; }

    public RegisterForm(RegisterController controller)
    {
        _controller = controller;
        BuildLayout();
    }

    private void BuildLayout()
    {
        Text = "StrongHub - Register";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        ClientSize = new Size(340, 290);

        string[] labels = { "Name", "Username", "Age", "Weight (kg)", "Height (m)" };
        TextBox[] boxes = { _nameBox, _usernameBox, _ageBox, _weightBox, _heightBox };
        for (int i = 0; i < labels.Length; i++)
        {
            int top = 20 + i * 36;
            Controls.Add(new Label { Text = labels[i], Location = new Point(20, top + 3), AutoSize = true });
            boxes[i].Location = new Point(130, top);
            boxes[i].Width = 180;
            Controls.Add(boxes[i]);
        }

        _bmiLabel.Location = new Point(130, 205);
        Controls.Add(_bmiLabel);
        _weightBox.TextChanged += (_, _) => UpdateBmiPreview();
        _heightBox.TextChanged += (_, _) => UpdateBmiPreview();

        var saveButton = new Button { Text = "Save", Location = new Point(130, 240), Size = new Size(85, 32) };
        var cancelButton = new Button { Text = "Cancel", Location = new Point(225, 240), Size = new Size(85, 32) };
        saveButton.Click += OnSaveClick;
        cancelButton.DialogResult = DialogResult.Cancel;
        AcceptButton = saveButton;
        CancelButton = cancelButton;
        Controls.Add(saveButton);
        Controls.Add(cancelButton);
    }

    private void UpdateBmiPreview()
    {
        double? bmi = _controller.PreviewBmi(_weightBox.Text, _heightBox.Text);
        _bmiLabel.Text = bmi.HasValue ? $"BMI: {bmi.Value:F2}" : "BMI: -";
    }

    private void OnSaveClick(object? sender, EventArgs e)
    {
        try
        {
            BasicUser user = _controller.Register(
                _nameBox.Text, _usernameBox.Text, _ageBox.Text, _weightBox.Text, _heightBox.Text);
            RegisteredUsername = user.Username;
            MessageBox.Show($"User registered. Your BMI is {user.Bmi:F2}.", "Registration complete",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
        }
        catch (ValidationException ex)
        {
            MessageBox.Show(ex.Message, "Invalid data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (DataAccessException ex)
        {
            MessageBox.Show(ex.Message, "Data error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
