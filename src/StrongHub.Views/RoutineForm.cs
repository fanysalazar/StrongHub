using System.ComponentModel;
using StrongHub.Controllers;
using StrongHub.Models;

namespace StrongHub.Views;

/// <summary>Form where a trainer creates or edits a routine.</summary>
public class RoutineForm : Form
{
    private readonly RoutineController _controller;
    private readonly int? _routineId;
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

    /// <param name="controller">Routine controller.</param>
    /// <param name="routineId">Id of the routine to edit, or null to create a new one.</param>
    public RoutineForm(RoutineController controller, int? routineId = null)
    {
        _controller = controller;
        _routineId = routineId;
        BuildLayout();
        LoadMembers();

        if (routineId is int id)
        {
            LoadRoutine(id);
        }
    }

    private bool IsEditing => _routineId.HasValue;

    private void BuildLayout()
    {
        Text = IsEditing ? "StrongHub - Edit routine" : "StrongHub - New routine";
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
        _grid.CellClick += (_, e) => FillInputsFromRow(e.RowIndex);
        Controls.Add(_grid);
        _grid.DataSource = _exercises;
        _grid.Columns["Name"]!.HeaderText = "Exercise";
        _grid.Columns["WeightKg"]!.HeaderText = "Weight (kg)";

        var updateButton = new Button { Text = "Update selected", Location = new Point(20, 480), Size = new Size(140, 32) };
        updateButton.Click += OnUpdateExerciseClick;
        Controls.Add(updateButton);

        var removeButton = new Button { Text = "Remove selected", Location = new Point(170, 480), Size = new Size(140, 32) };
        removeButton.Click += OnRemoveClick;
        Controls.Add(removeButton);

        var saveButton = new Button
        {
            Text = IsEditing ? "Save changes" : "Save routine",
            Location = new Point(320, 530),
            Size = new Size(120, 36)
        };
        var cancelButton = new Button { Text = "Cancel", Location = new Point(450, 530), Size = new Size(90, 36) };
        saveButton.Click += OnSaveClick;
        cancelButton.DialogResult = DialogResult.Cancel;
        CancelButton = cancelButton;
        Controls.Add(saveButton);
        Controls.Add(cancelButton);
    }

    private GroupBox BuildExerciseGroup()
    {
        var group = new GroupBox { Text = "Exercise details", Location = new Point(20, 170), Size = new Size(520, 105) };

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

    private void LoadRoutine(int routineId)
    {
        Routine routine = _controller.GetRoutineForEdit(routineId);

        _memberBox.SelectedValue = routine.UserId;
        _memberBox.Enabled = false;
        _nameBox.Text = routine.Name;
        _startPicker.Value = routine.StartDate.ToDateTime(TimeOnly.MinValue);
        _endPicker.Value = routine.EndDate.ToDateTime(TimeOnly.MinValue);

        foreach (Exercise exercise in routine.Exercises)
        {
            _exercises.Add(exercise);
        }
    }

    private void UpdateExistingLabel()
    {
        _existingLabel.Text = _memberBox.SelectedValue is int memberId
            ? $"Existing routines for this member: {_controller.CountRoutinesOf(memberId)}"
            : string.Empty;
    }

    private void FillInputsFromRow(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= _exercises.Count)
        {
            return;
        }

        Exercise exercise = _exercises[rowIndex];
        _exerciseNameBox.Text = exercise.Name;
        _setsBox.Text = exercise.Sets.ToString();
        _repsBox.Text = exercise.Reps.ToString();
        _weightBox.Text = exercise.WeightKg.ToString(System.Globalization.CultureInfo.InvariantCulture);
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

    private void OnUpdateExerciseClick(object? sender, EventArgs e)
    {
        if (_grid.CurrentRow?.DataBoundItem is not Exercise selected)
        {
            MessageBox.Show("Click an exercise in the table first.", "Update exercise",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            Exercise changed = _controller.ParseExercise(
                _exerciseNameBox.Text, _setsBox.Text, _repsBox.Text, _weightBox.Text);
            _exercises[_exercises.IndexOf(selected)] = changed;
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
            MessageBox.Show("Select a member first.", "Routine", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            var startDate = DateOnly.FromDateTime(_startPicker.Value);
            var endDate = DateOnly.FromDateTime(_endPicker.Value);

            Routine routine = _routineId is int id
                ? _controller.Update(id, _nameBox.Text, startDate, endDate, _exercises.ToList())
                : _controller.Create(memberId, _nameBox.Text, startDate, endDate, _exercises.ToList());

            MessageBox.Show($"Routine '{routine.Name}' saved with {routine.Exercises.Count} exercise(s).",
                "Routine saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
        }
        catch (ValidationException ex)
        {
            MessageBox.Show(ex.Message, "Invalid routine", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (RoutineNotFoundException ex)
        {
            MessageBox.Show(ex.Message, "Routine not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            DialogResult = DialogResult.Cancel;
        }
        catch (DataAccessException ex)
        {
            MessageBox.Show(ex.Message, "Data error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
