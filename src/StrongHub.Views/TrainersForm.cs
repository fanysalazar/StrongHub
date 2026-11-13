using StrongHub.Controllers;
using StrongHub.Models;

namespace StrongHub.Views;

/// <summary>Lists trainers with their type and lets a member pick one.</summary>
public class TrainersForm : Form
{
    private readonly TrainerController _controller;
    private readonly DataGridView _grid = new();
    private readonly ComboBox _typeFilter = new();
    private readonly Label _assignedLabel = new() { AutoSize = true };
    private readonly Button _assignButton = new() { Text = "Assign selected trainer" };

    public TrainersForm(TrainerController controller)
    {
        _controller = controller;
        BuildLayout();
        LoadGrid();
        _assignedLabel.Text = _controller.GetAssignedTrainerText();
    }

    private void BuildLayout()
    {
        Text = "StrongHub - Trainers";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        ClientSize = new Size(560, 470);

        Controls.Add(new Label { Text = "Type", AutoSize = true, Location = new Point(20, 23) });

        _typeFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        _typeFilter.Items.AddRange(new object[] { "All", "Personal", "Gym" });
        _typeFilter.SelectedIndex = 0;
        _typeFilter.Location = new Point(70, 20);
        _typeFilter.Width = 140;
        _typeFilter.SelectedIndexChanged += (_, _) => LoadGrid();
        Controls.Add(_typeFilter);

        _grid.Location = new Point(20, 60);
        _grid.Size = new Size(520, 300);
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.MultiSelect = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        Controls.Add(_grid);

        _assignedLabel.Location = new Point(20, 375);
        Controls.Add(_assignedLabel);

        _assignButton.Location = new Point(20, 415);
        _assignButton.Size = new Size(190, 36);
        _assignButton.Enabled = _controller.CanAssign;
        _assignButton.Click += OnAssignClick;
        Controls.Add(_assignButton);

        var closeButton = new Button { Text = "Close", Location = new Point(450, 415), Size = new Size(90, 36) };
        closeButton.DialogResult = DialogResult.Cancel;
        CancelButton = closeButton;
        Controls.Add(closeButton);
    }

    private void LoadGrid()
    {
        TrainerType? type = _typeFilter.SelectedIndex switch
        {
            1 => TrainerType.Personal,
            2 => TrainerType.Gym,
            _ => null
        };

        _grid.DataSource = _controller.GetTrainerRows(type).ToList();
    }

    private void OnAssignClick(object? sender, EventArgs e)
    {
        if (_grid.SelectedRows.Count == 0)
        {
            MessageBox.Show("Select a trainer first.", "Trainers", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            int trainerId = (int)_grid.SelectedRows[0].Cells["Id"].Value;
            _controller.AssignTrainer(trainerId);
            _assignedLabel.Text = _controller.GetAssignedTrainerText();
        }
        catch (ValidationException ex)
        {
            MessageBox.Show(ex.Message, "Trainers", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (DataAccessException ex)
        {
            MessageBox.Show(ex.Message, "Data error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
