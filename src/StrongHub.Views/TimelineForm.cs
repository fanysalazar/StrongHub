using StrongHub.Controllers;
using StrongHub.Models;

namespace StrongHub.Views;

/// <summary>Shows the notifications of the logged-in user, newest first. Trainers can also set the alert threshold.</summary>
public class TimelineForm : Form
{
    private readonly TimelineController _controller;
    private readonly TrainerAlertController _alerts;
    private readonly DataGridView _grid = new();
    private readonly Label _summaryLabel = new() { AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
    private readonly NumericUpDown _thresholdBox = new();

    public TimelineForm(TimelineController controller, TrainerAlertController alerts)
    {
        _controller = controller;
        _alerts = alerts;
        BuildLayout();
        LoadGrid();
        _controller.MarkAllSeen();
    }

    private void BuildLayout()
    {
        Text = "StrongHub - Timeline";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        ClientSize = new Size(760, 470);

        _summaryLabel.Location = new Point(20, 20);
        Controls.Add(_summaryLabel);

        int gridTop = 55;
        if (_alerts.CanConfigure)
        {
            BuildThresholdControls();
            gridTop = 100;
        }

        _grid.Location = new Point(20, gridTop);
        _grid.Size = new Size(720, 405 - gridTop);
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.MultiSelect = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        Controls.Add(_grid);

        var closeButton = new Button { Text = "Close", Location = new Point(650, 420), Size = new Size(90, 36) };
        closeButton.DialogResult = DialogResult.Cancel;
        CancelButton = closeButton;
        Controls.Add(closeButton);
    }

    private void BuildThresholdControls()
    {
        Controls.Add(new Label { Text = "Alert threshold (days)", AutoSize = true, Location = new Point(20, 63) });

        _thresholdBox.Minimum = TrainerAlertController.MinThresholdDays;
        _thresholdBox.Maximum = TrainerAlertController.MaxThresholdDays;
        _thresholdBox.Value = _alerts.ThresholdDays;
        _thresholdBox.Location = new Point(170, 60);
        _thresholdBox.Width = 70;
        Controls.Add(_thresholdBox);

        var saveButton = new Button { Text = "Save and check now", Location = new Point(255, 55), Size = new Size(170, 32) };
        saveButton.Click += OnSaveThresholdClick;
        Controls.Add(saveButton);
    }

    private void LoadGrid()
    {
        _summaryLabel.Text = _controller.GetSummary();
        _grid.DataSource = _controller.GetRows().ToList();
        SetHeader("When", "Date", 18);
        SetHeader("Type", "Type", 16);
        SetHeader("Message", "Message", 66);
    }

    private void SetHeader(string column, string text, float weight)
    {
        if (_grid.Columns.Contains(column))
        {
            _grid.Columns[column]!.HeaderText = text;
            _grid.Columns[column]!.FillWeight = weight;
        }
    }

    private void OnSaveThresholdClick(object? sender, EventArgs e)
    {
        try
        {
            AlertRefreshResult result = _alerts.SetThresholdAndRefresh((int)_thresholdBox.Value);
            LoadGrid();
            _controller.MarkAllSeen();

            MessageBox.Show(
                $"Threshold saved. New alerts: {result.RoutineAlerts} routine(s), {result.MeasurementAlerts} measurement(s).",
                "Alerts", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (ValidationException ex)
        {
            MessageBox.Show(ex.Message, "Alerts", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (DataAccessException ex)
        {
            MessageBox.Show(ex.Message, "Data error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
