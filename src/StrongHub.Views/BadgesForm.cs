using StrongHub.Controllers;
using StrongHub.Models;

namespace StrongHub.Views;

/// <summary>Shows the badges of the logged-in member and awards the new ones.</summary>
public class BadgesForm : Form
{
    private readonly BadgeController _controller;
    private readonly DataGridView _grid = new();
    private readonly Label _summaryLabel = new() { AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

    public BadgesForm(BadgeController controller)
    {
        _controller = controller;
        BuildLayout();
        LoadGrid();
        Shown += (_, _) => CheckForNewBadges(announceWhenNone: false);
    }

    private void BuildLayout()
    {
        Text = "StrongHub - My badges";
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

        var checkButton = new Button { Text = "Check for new badges", Location = new Point(20, 420), Size = new Size(190, 36) };
        checkButton.Click += (_, _) => CheckForNewBadges(announceWhenNone: true);
        Controls.Add(checkButton);

        var closeButton = new Button { Text = "Close", Location = new Point(610, 420), Size = new Size(90, 36) };
        closeButton.DialogResult = DialogResult.Cancel;
        CancelButton = closeButton;
        Controls.Add(closeButton);
    }

    private void LoadGrid()
    {
        _summaryLabel.Text = _controller.GetSummary();
        _grid.DataSource = _controller.GetMyBadges().ToList();
        SetHeader("Name", "Badge");
        SetHeader("Type", "Type");
        SetHeader("Date", "Date");
        SetHeader("Message", "Message");
    }

    private void SetHeader(string column, string text)
    {
        if (_grid.Columns.Contains(column))
        {
            _grid.Columns[column]!.HeaderText = text;
        }
    }

    private void CheckForNewBadges(bool announceWhenNone)
    {
        try
        {
            IReadOnlyList<UserBadge> awarded = _controller.EvaluateAchievements();
            LoadGrid();

            if (awarded.Count > 0)
            {
                string names = string.Join("\n- ", awarded.Select(badge => badge.BadgeName));
                MessageBox.Show($"New badge(s) earned:\n- {names}", "Congratulations",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (announceWhenNone)
            {
                MessageBox.Show("No new badges yet. Keep training!", "My badges",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (ValidationException ex)
        {
            MessageBox.Show(ex.Message, "My badges", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (DataAccessException ex)
        {
            MessageBox.Show(ex.Message, "Data error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
