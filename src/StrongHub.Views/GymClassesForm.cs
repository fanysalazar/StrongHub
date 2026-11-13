using StrongHub.Controllers;

namespace StrongHub.Views;

/// <summary>Shows the catalog of classes offered by the gym.</summary>
public class GymClassesForm : Form
{
    private readonly ClassCatalogController _controller;
    private readonly DataGridView _grid = new();
    private readonly TextBox _detailBox = new();

    public GymClassesForm(ClassCatalogController controller)
    {
        _controller = controller;
        BuildLayout();
        LoadGrid();
    }

    private void BuildLayout()
    {
        Text = "StrongHub - Gym classes";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        ClientSize = new Size(620, 440);

        _grid.Location = new Point(20, 20);
        _grid.Size = new Size(580, 220);
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.MultiSelect = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.SelectionChanged += (_, _) => ShowDetail();
        Controls.Add(_grid);

        Controls.Add(new Label { Text = "Description", AutoSize = true, Location = new Point(20, 255) });

        _detailBox.Location = new Point(20, 280);
        _detailBox.Size = new Size(580, 90);
        _detailBox.Multiline = true;
        _detailBox.ReadOnly = true;
        Controls.Add(_detailBox);

        var closeButton = new Button { Text = "Close", Location = new Point(510, 385), Size = new Size(90, 36) };
        closeButton.DialogResult = DialogResult.Cancel;
        CancelButton = closeButton;
        Controls.Add(closeButton);
    }

    private void LoadGrid()
    {
        _grid.DataSource = _controller.GetCatalog().ToList();
        _grid.Columns["Id"]!.Visible = false;
        _grid.Columns["Description"]!.Visible = false;
        _grid.Columns["Name"]!.HeaderText = "Class";
        _grid.Columns["DurationMinutes"]!.HeaderText = "Minutes";
        _grid.Columns["TrainingFocus"]!.HeaderText = "Focus";
        ShowDetail();
    }

    private void ShowDetail()
    {
        bool hasSelection = _grid.SelectedRows.Count > 0
                            && _grid.SelectedRows[0].DataBoundItem is CatalogEntry;
        _detailBox.Text = hasSelection
            ? ((CatalogEntry)_grid.SelectedRows[0].DataBoundItem!).Description
            : string.Empty;
    }
}
