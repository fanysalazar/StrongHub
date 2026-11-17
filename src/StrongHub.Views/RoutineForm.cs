using System.ComponentModel;
using StrongHub.Controllers;
using StrongHub.Models;

namespace StrongHub.Views;

/// <summary>Form where a trainer creates a routine for a member.</summary>
public class RoutineForm : Form
{
    private readonly RoutineController _controller;
    private readonly ComboBox _memberBox = new();
    private readonly Label _existingLabel = new() { AutoSize = true };
    private readonly TextBox _nameBox = new();
    private readonly DateTimePicker _startPicker = new();
    private readonly DateTimePicker _endPicker = new();
    private readonly TextBox _exerciseNameBox = new();
    private readonly TextBox _setsBox = new();
    private readonly TextBox _repsBox = new();
    private readonly TextBox _weightBox = new();
    private readonly BindingList<Exercise> _exercises = new();
    private readonly DataGridView _grid = new();

    public RoutineForm(RoutineController controller)
    {
        _controller = controller;
        BuildLayout();
        LoadMembers();
    }

    private void BuildLayout()
    {
        Text = "StrongHub - New routine";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        ClientSize = new Size(560, 580);

        AddLabel("Member", 20, 23);
        _memberBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _memberBox.Location = new Point(110, 20);
        _memberBox.Width = 430;
        _memberBox.SelectedIndexChanged += (_, _) => UpdateExistingLabel();
        Controls.Add(_memberBox);

        _existingLabel.Location = new Point(110, 55);
        Controls.Add(_existingLabel);

        AddLabel("Routine name", 20, 93);
        _nameBox.Location = new Point(110, 90);
        _nameBox.Width = 430;
        Controls.Add(_nameBox);

        AddLabel("Start date", 20, 128);
        _startPicker.Format = DateTimePickerFormat.Short;
        _startPicker.Location = new Point(110, 125);
        _startPicker.Width = 130;
        _startPicker.Value = DateTime.Today;
        Controls.Add(_startPicker);

        AddLabel("End date", 270, 128);
        _endPicker.Format = DateTimePickerFormat.Short;
        _endPicker.Location = new Point(330, 125);
        _endPicker.Width = 130;
        _endPicker.Value = DateTime.Today.AddDays(28);
        Controls.Add(_endPicker);

        Controls.Add(BuildExerciseGroup());

        _grid.Location = new Point(20, 290);
        _grid.Size = new Size(520, 180);
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.MultiSelect = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        Controls.Add(_grid);
        _grid.DataSource = _exercises;
        _grid.Columns["Name"]!.HeaderText = "Exercise";
        _grid.Columns["WeightKg"]!.HeaderText = "Weight (kg)";
        Controls.Add(_grid);

        var removeButton = new Button { Text = "Remove selected", Location = new Point(20, 480), Size = new Size(150, 32) };
        removeButton.Click += OnRemoveClick;
        Controls.Add(removeButton);

        var saveButton = new Button { Text = "Save routine", Location = new Point(330, 530), Size = new Size(110, 36) };
        var cancelButton = new Button { Text = "Cancel", Location = new Point(450, 530), Size = new Size(90, 36) };
        saveButton.Click += OnSaveClick;
        cancelButton.DialogResult = DialogResult.Cancel;
        CancelButton = cancelButton;
        Controls.Add(saveButton);
        Controls.Add(cancelButton);
    }

    private GroupBox BuildExerciseGroup()
    {
        var group = new GroupBox { Text = "Add exercise", Location = new Point(20, 170), Size = new Size(520, 105) };

        group.Controls.Add(new Label { Text = "Exercise", AutoSize = true, Location = new Point(15, 30) });
        _exerciseNameBox.Location = new Point(80, 27);
        _exerciseNameBox.Width = 420;
        group.Controls.Add(_exerciseNameBox);

        group.Controls.Add(new Label { Text = "Sets", AutoSize = true, Location = new Point(15, 68) });
        _setsBox.Location = new Point(55, 65);
        _setsBox.Width = 55;
        group.Controls.Add(_setsBox);

        group.Controls.Add(new Label { Text = "Reps", AutoSize = true, Location = new Point(125, 68) });
        _repsBox.Location = new Point(165, 65);
        _repsBox.Width = 55;
        group.Controls.Add(_repsBox);

        group.Controls.Add(new Label { Text = "Weight (kg)", AutoSize = true, Location = new Point(235, 68) });
        _weightBox.Location = new Point(315, 65);
        _weightBox.Width = 70;
        group.Controls.Add(_weightBox);

        var addButton = new Button { Text = "Add", Location = new Point(410, 60), Size = new Size(90, 32) };
        addButton.Click += OnAddExerciseClick;
        group.Controls.Add(addButton);

        return group;
    }

    private void AddLabel(string text, int left, int top) =>
        Controls.Add(new Label { Text = text, AutoSize = true, Location = new Point(left, top) });

    private void LoadMembers()
    {
        _memberBox.DisplayMember = nameof(MemberOption.Label);
        _memberBox.ValueMember = nameof(MemberOption.Id);
        _memberBox.DataSource = _controller.GetMemberOptions().ToList();
        UpdateExistingLabel();
    }

    private void UpdateExistingLabel()
    {
        _existingLabel.Text = _memberBox.SelectedValue is int memberId
            ? $"Existing routines for this member: {_controller.CountRoutinesOf(memberId)}"
            : string.Empty;
    }

    private void OnAddExerciseClick(object? sender, EventArgs e)
    {
        try
        {
            Exercise exercise = _controller.ParseExercise(
                _exerciseNameBox.Text, _setsBox.Text, _repsBox.Text, _weightBox.Text);
            _exercises.Add(exercise);
            ClearExerciseInputs();
        }
        catch (ValidationException ex)
        {
            MessageBox.Show(ex.Message, "Invalid exercise", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ClearExerciseInputs()
    {
        _exerciseNameBox.Clear();
        _setsBox.Clear();
        _repsBox.Clear();
        _weightBox.Clear();
        _exerciseNameBox.Focus();
    }

    private void OnRemoveClick(object? sender, EventArgs e)
    {
        if (_grid.CurrentRow?.DataBoundItem is Exercise exercise)
        {
            _exercises.Remove(exercise);
        }
    }

    private void OnSaveClick(object? sender, EventArgs e)
    {
        if (_memberBox.SelectedValue is not int memberId)
        {
            MessageBox.Show("Select a member first.", "New routine", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            Routine routine = _controller.Create(
                memberId,
                _nameBox.Text,
                DateOnly.FromDateTime(_startPicker.Value),
                DateOnly.FromDateTime(_endPicker.Value),
                _exercises.ToList());

            MessageBox.Show($"Routine '{routine.Name}' saved with {routine.Exercises.Count} exercise(s).",
                "Routine saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
        }
        catch (ValidationException ex)
        {
            MessageBox.Show(ex.Message, "Invalid routine", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (DataAccessException ex)
        {
            MessageBox.Show(ex.Message, "Data error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
