using StrongHub.Controllers;
using StrongHub.Models;

namespace StrongHub.Views;

/// <summary>Lets a member register visits and see the visit history.</summary>
public class VisitForm : Form
{
    private readonly VisitController _controller;
    private readonly ComboBox _classBox = new();
    private readonly DateTimePicker _datePicker = new();
    private readonly Label _weekLabel = new() { AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
    private readonly DataGridView _grid = new();
    private readonly Button _registerButton = new() { Text = "Register visit" };

    public VisitForm(VisitController controller)
    {
        _controller = controller;
        BuildLayout();
        LoadClasses();
        Refresh();
    }

    private void BuildLayout()
    {
        Text = "StrongHub - Register visit";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        ClientSize = new Size(560, 520);

        Controls.Add(new Label { Text = "Class", AutoSize = true, Location = new Point(20, 23) });
        _classBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _classBox.Location = new Point(70, 20);
        _classBox.Width = 180;
        Controls.Add(_classBox);

        Controls.Add(new Label { Text = "Date", AutoSize = true, Location = new Point(275, 23) });
        _datePicker.Format = DateTimePickerFormat.Short;
        _datePicker.Location = new Point(320, 20);
        _datePicker.Width = 120;
        _datePicker.MaxDate = _controller.Today.ToDateTime(TimeOnly.MinValue);
        _datePicker.Value = _controller.Today.ToDateTime(TimeOnly.MinValue);
        Controls.Add(_datePicker);

        _registerButton.Location = new Point(20, 60);
        _registerButton.Size = new Size(140, 34);
        _registerButton.Enabled = _controller.CanRegister;
        _registerButton.Click += OnRegisterClick;
        Controls.Add(_registerButton);

        _weekLabel.Location = new Point(20, 110);
        Controls.Add(_weekLabel);

        _grid.Location = new Point(20, 145);
        _grid.Size = new Size(520, 310);
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.MultiSelect = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        Controls.Add(_grid);

        var closeButton = new Button { Text = "Close", Location = new Point(450, 470), Size = new Size(90, 36) };
        closeButton.DialogResult = DialogResult.Cancel;
        CancelButton = closeButton;
        Controls.Add(closeButton);
    }

    private void LoadClasses()
    {
        _classBox.DataSource = _controller.GetClassNames().ToList();
    }

    private new void Refresh()
    {
        _weekLabel.Text = _controller.GetWeekSummary();
        _grid.DataSource = _controller.GetHistoryRows().ToList();

        if (_grid.Columns.Contains("Id"))
        {
            _grid.Columns["Id"]!.Visible = false;
        }

        if (_grid.Columns.Contains("Date"))
        {
            _grid.Columns["Date"]!.HeaderText = "Date";
        }

        if (_grid.Columns.Contains("ClassName"))
        {
            _grid.Columns["ClassName"]!.HeaderText = "Class";
        }
    }

    private void OnRegisterClick(object? sender, EventArgs e)
    {
        try
        {
            VisitResult result = _controller.Register(
                _classBox.Text, DateOnly.FromDateTime(_datePicker.Value));
            Refresh();

            if (result.RestRecommended)
            {
                MessageBox.Show(
                    $"You have reached {VisitController.RestThreshold} visits this week. Rest this week!",
                    "Rest recommended", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (ValidationException ex)
        {
            MessageBox.Show(ex.Message, "Invalid visit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (DataAccessException ex)
        {
            MessageBox.Show(ex.Message, "Data error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
