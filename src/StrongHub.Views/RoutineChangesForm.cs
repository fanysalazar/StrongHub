using StrongHub.Controllers;

namespace StrongHub.Views;

/// <summary>Shows how the loads of the member's routines have changed over time.</summary>
public class RoutineChangesForm : Form
{
    private readonly RoutineChangesController _controller;
    private readonly DataGridView _grid = new();
    private readonly Label _summaryLabel = new() { AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

    public RoutineChangesForm(RoutineChangesController controller)
    {
        _controller = controller;
        BuildLayout();
        LoadGrid();
    }

    private void BuildLayout()
    {
        Text = "StrongHub - Load increases";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        ClientSize = new Size(720, 470);

        _summaryLabel.Location = new Point(20, 20);
        Controls.Add(_summaryLabel);

        _grid.Location = new Point(20, 55);
        _grid.Size = new Size(680, 350);
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.MultiSelect = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        Controls.Add(_grid);

        var closeButton = new Button { Text = "Close", Location = new Point(610, 420), Size = new Size(90, 36) };
        closeButton.DialogResult = DialogResult.Cancel;
        CancelButton = closeButton;
        Controls.Add(closeButton);
    }

    private void LoadGrid()
    {
        _summaryLabel.Text = _controller.GetSummary();
        _grid.DataSource = _controller.GetRows().ToList();
        SetHeader("Date", "Date");
        SetHeader("Routine", "Routine");
        SetHeader("Exercise", "Exercise");
        SetHeader("PreviousKg", "Previous (kg)");
        SetHeader("NewKg", "New (kg)");
        SetHeader("Change", "Change (kg)");
    }

    private void SetHeader(string column, string text)
    {
        if (_grid.Columns.Contains(column))
        {
            _grid.Columns[column]!.HeaderText = text;
        }
    }
}
