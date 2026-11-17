using StrongHub.Controllers;
using StrongHub.Models;

namespace StrongHub.Views;

/// <summary>Lets a trainer assign shame badges for a period and see who received them.</summary>
public class ShameBadgesForm : Form
{
    private readonly ShameBadgeController _controller;
    private readonly DateTimePicker _startPicker = new();
    private readonly DateTimePicker _endPicker = new();
    private readonly DataGridView _grid = new();
    private readonly Label _summaryLabel = new() { AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
    private readonly Button _assignButton = new() { Text = "Evaluate and assign" };

    public ShameBadgesForm(ShameBadgeController controller)
    {
        _controller = controller;
        BuildLayout();
    }

    private void BuildLayout()
    {
        Text = "StrongHub - Shame badges";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        ClientSize = new Size(820, 540);

        DateTime today = _controller.Today.ToDateTime(TimeOnly.MinValue);

        Controls.Add(new Label { Text = "From", AutoSize = true, Location = new Point(20, 23) });
        ConfigurePicker(_startPicker, new Point(65, 20), today, today.AddDays(-29));
        Controls.Add(_startPicker);

        Controls.Add(new Label { Text = "To", AutoSize = true, Location = new Point(205, 23) });
        ConfigurePicker(_endPicker, new Point(230, 20), today, today);
        Controls.Add(_endPicker);

        _assignButton.Location = new Point(380, 15);
        _assignButton.Size = new Size(170, 34);
        _assignButton.Enabled = _controller.CanEvaluate;
        _assignButton.Click += OnAssignClick;
        Controls.Add(_assignButton);

        var showButton = new Button { Text = "Show existing", Location = new Point(560, 15), Size = new Size(130, 34) };
        showButton.Click += OnShowClick;
        Controls.Add(showButton);

        _summaryLabel.Location = new Point(20, 65);
        _summaryLabel.Text = "Choose a period and press 'Evaluate and assign'.";
        Controls.Add(_summaryLabel);

        _grid.Location = new Point(20, 100);
        _grid.Size = new Size(780, 380);
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.MultiSelect = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        Controls.Add(_grid);

        var closeButton = new Button { Text = "Close", Location = new Point(710, 490), Size = new Size(90, 36) };
        closeButton.DialogResult = DialogResult.Cancel;
        CancelButton = closeButton;
        Controls.Add(closeButton);
    }

    private static void ConfigurePicker(DateTimePicker picker, Point location, DateTime max, DateTime value)
    {
        picker.Format = DateTimePickerFormat.Short;
        picker.Location = location;
        picker.Width = 120;
        picker.MaxDate = max;
        picker.Value = value;
    }

    private DateOnly Start => DateOnly.FromDateTime(_startPicker.Value);

    private DateOnly End => DateOnly.FromDateTime(_endPicker.Value);

    private void ShowRows(IReadOnlyList<ShameRow> rows)
    {
        _grid.DataSource = rows.ToList();

        if (_grid.Columns.Contains("Visits"))
        {
            _grid.Columns["Visits"]!.HeaderText = "Visits in period";
        }
    }

    private void OnAssignClick(object? sender, EventArgs e)
    {
        try
        {
            ShameResult result = _controller.Evaluate(Start, End);
            ShowRows(result.Rows);
            _summaryLabel.Text =
                $"{result.NewlyAssigned} new badge(s) assigned. {result.Rows.Count} in total for this period.";
        }
        catch (ValidationException ex)
        {
            MessageBox.Show(ex.Message, "Shame badges", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (DataAccessException ex)
        {
            MessageBox.Show(ex.Message, "Data error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnShowClick(object? sender, EventArgs e)
    {
        try
        {
            IReadOnlyList<ShameRow> rows = _controller.GetAssignedRows(Start, End);
            ShowRows(rows);
            _summaryLabel.Text = $"{rows.Count} badge(s) already assigned for this period.";
        }
        catch (ValidationException ex)
        {
            MessageBox.Show(ex.Message, "Shame badges", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
