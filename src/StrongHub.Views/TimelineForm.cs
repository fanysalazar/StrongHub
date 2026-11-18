using StrongHub.Controllers;

namespace StrongHub.Views;

/// <summary>Shows the notifications of the logged-in user, newest first.</summary>
public class TimelineForm : Form
{
    private readonly TimelineController _controller;
    private readonly DataGridView _grid = new();
    private readonly Label _summaryLabel = new() { AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

    public TimelineForm(TimelineController controller)
    {
        _controller = controller;
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

        _grid.Location = new Point(20, 55);
        _grid.Size = new Size(720, 350);
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
}
