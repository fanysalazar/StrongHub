using StrongHub.Controllers;
using StrongHub.Models;

namespace StrongHub.Views;

/// <summary>Lists routines and lets a trainer create, edit or delete them.</summary>
public class RoutineListForm : Form
{
    private readonly RoutineController _controller;
    private readonly ComboBox _memberFilter = new();
    private readonly CheckBox _onlyMineBox = new() { Text = "Only routines I created", AutoSize = true };
    private readonly DataGridView _grid = new();
    private readonly Label _countLabel = new() { AutoSize = true };

    public RoutineListForm(RoutineController controller)
    {
        _controller = controller;
        BuildLayout();
        LoadFilter();
        LoadGrid();
    }

    private void BuildLayout()
    {
        Text = "StrongHub - Manage routines";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        ClientSize = new Size(780, 540);

        Controls.Add(new Label { Text = "Member", AutoSize = true, Location = new Point(20, 23) });

        _memberFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        _memberFilter.Location = new Point(85, 20);
        _memberFilter.Width = 330;
        Controls.Add(_memberFilter);

        _onlyMineBox.Location = new Point(440, 22);
        _onlyMineBox.CheckedChanged += (_, _) => LoadGrid();
        Controls.Add(_onlyMineBox);

        _grid.Location = new Point(20, 60);
        _grid.Size = new Size(740, 380);
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.MultiSelect = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0)
            {
                OnEditClick(null, EventArgs.Empty);
            }
        };
        Controls.Add(_grid);

        _countLabel.Location = new Point(20, 450);
        Controls.Add(_countLabel);

        var newButton = new Button { Text = "New routine", Location = new Point(20, 485), Size = new Size(130, 36) };
        var editButton = new Button { Text = "Edit selected", Location = new Point(160, 485), Size = new Size(130, 36) };
        var deleteButton = new Button { Text = "Delete selected", Location = new Point(300, 485), Size = new Size(130, 36) };
        var closeButton = new Button { Text = "Close", Location = new Point(670, 485), Size = new Size(90, 36) };
        newButton.Click += OnNewClick;
        editButton.Click += OnEditClick;
        deleteButton.Click += OnDeleteClick;
        closeButton.DialogResult = DialogResult.Cancel;
        CancelButton = closeButton;
        Controls.Add(newButton);
        Controls.Add(editButton);
        Controls.Add(deleteButton);
        Controls.Add(closeButton);
    }

    private void LoadFilter()
    {
        var options = new List<MemberOption> { new(0, "All members") };
        options.AddRange(_controller.GetMemberOptions());

        _memberFilter.DisplayMember = nameof(MemberOption.Label);
        _memberFilter.ValueMember = nameof(MemberOption.Id);
        _memberFilter.DataSource = options;
        _memberFilter.SelectedIndex = 0;
        _memberFilter.SelectedIndexChanged += (_, _) => LoadGrid();
    }

    private void LoadGrid()
    {
        int? memberId = _memberFilter.SelectedValue is int id && id != 0 ? id : null;
        IReadOnlyList<RoutineRow> rows = _controller.GetRoutineRows(memberId, _onlyMineBox.Checked);

        _grid.DataSource = rows.ToList();
        SetHeader("Id", "ID");
        SetHeader("Member", "Member");
        SetHeader("Name", "Routine");
        SetHeader("Start", "Start");
        SetHeader("End", "End");
        SetHeader("Exercises", "Exercises");
        _countLabel.Text = $"{rows.Count} routine(s)";
    }

    private void SetHeader(string column, string text)
    {
        if (_grid.Columns.Contains(column))
        {
            _grid.Columns[column]!.HeaderText = text;
        }
    }

    private RoutineRow? SelectedRow => _grid.CurrentRow?.DataBoundItem as RoutineRow;

    private void OnNewClick(object? sender, EventArgs e)
    {
        using var form = new RoutineForm(_controller);
        form.ShowDialog(this);
        LoadGrid();
    }

    private void OnEditClick(object? sender, EventArgs e)
    {
        RoutineRow? row = SelectedRow;
        if (row is null)
        {
            MessageBox.Show("Select a routine first.", "Edit routine", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            using var form = new RoutineForm(_controller, row.Id);
            form.ShowDialog(this);
        }
        catch (RoutineNotFoundException ex)
        {
            MessageBox.Show(ex.Message, "Routine not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        LoadGrid();
    }

    private void OnDeleteClick(object? sender, EventArgs e)
    {
        RoutineRow? row = SelectedRow;
        if (row is null)
        {
            MessageBox.Show("Select a routine first.", "Delete routine", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        DialogResult answer = MessageBox.Show(
            $"Delete routine '{row.Name}' of {row.Member}?\nThis cannot be undone.",
            "Delete routine",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (answer != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _controller.Delete(row.Id);
        }
        catch (RoutineNotFoundException ex)
        {
            MessageBox.Show(ex.Message, "Routine not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (ValidationException ex)
        {
            MessageBox.Show(ex.Message, "Delete routine", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (DataAccessException ex)
        {
            MessageBox.Show(ex.Message, "Data error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        LoadGrid();
    }
}
