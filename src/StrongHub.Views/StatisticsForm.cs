using StrongHub.Controllers;
using StrongHub.Models;

namespace StrongHub.Views;

/// <summary>Statistics of the logged-in member for a chosen period, calculated when shown.</summary>
public class StatisticsForm : Form
{
    private readonly StatisticsController _controller;
    private readonly DateTimePicker _startPicker = new();
    private readonly DateTimePicker _endPicker = new();
    private readonly Label _evolutionLabel = NewSummaryLabel();
    private readonly Label _rankingLabel = NewSummaryLabel();
    private readonly DataGridView _evolutionGrid = NewGrid(new Point(10, 45), new Size(700, 340));
    private readonly DataGridView _mostGrid = NewGrid(new Point(10, 80), new Size(345, 300));
    private readonly DataGridView _fewestGrid = NewGrid(new Point(365, 80), new Size(345, 300));

    public StatisticsForm(StatisticsController controller)
    {
        _controller = controller;
        BuildLayout();
        Shown += (_, _) => Calculate();
    }

    private void BuildLayout()
    {
        Text = "StrongHub - Statistics";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        ClientSize = new Size(760, 560);

        DateTime today = _controller.Today.ToDateTime(TimeOnly.MinValue);

        Controls.Add(new Label { Text = "From", AutoSize = true, Location = new Point(20, 23) });
        ConfigurePicker(_startPicker, new Point(65, 20), today, today.AddDays(-29));
        Controls.Add(_startPicker);

        Controls.Add(new Label { Text = "To", AutoSize = true, Location = new Point(205, 23) });
        ConfigurePicker(_endPicker, new Point(230, 20), today, today);
        Controls.Add(_endPicker);

        var calculateButton = new Button { Text = "Calculate", Location = new Point(380, 15), Size = new Size(130, 34) };
        calculateButton.Click += (_, _) => Calculate();
        Controls.Add(calculateButton);

        Prepare(_evolutionGrid, new Dictionary<string, string>
        {
            ["Metric"] = "Metric", ["Start"] = "Start", ["End"] = "End",
            ["Change"] = "Change", ["Goal"] = "Goal", ["Progress"] = "Progress"
        });
        Prepare(_mostGrid, RankingHeaders());
        Prepare(_fewestGrid, RankingHeaders());

        var tabs = new TabControl { Location = new Point(15, 65), Size = new Size(730, 440) };
        tabs.TabPages.Add(BuildEvolutionTab());
        tabs.TabPages.Add(BuildRankingTab());
        Controls.Add(tabs);

        var closeButton = new Button { Text = "Close", Location = new Point(655, 515), Size = new Size(90, 36) };
        closeButton.DialogResult = DialogResult.Cancel;
        CancelButton = closeButton;
        Controls.Add(closeButton);
    }

    private TabPage BuildEvolutionTab()
    {
        var page = new TabPage("Weight and goals");
        _evolutionLabel.Location = new Point(10, 12);
        page.Controls.Add(_evolutionLabel);
        page.Controls.Add(_evolutionGrid);
        return page;
    }

    private TabPage BuildRankingTab()
    {
        var page = new TabPage("Top 10 visits");
        _rankingLabel.Location = new Point(10, 12);
        page.Controls.Add(_rankingLabel);
        page.Controls.Add(new Label { Text = "Most visits", AutoSize = true, Location = new Point(10, 55) });
        page.Controls.Add(new Label { Text = "Fewest visits", AutoSize = true, Location = new Point(365, 55) });
        page.Controls.Add(_mostGrid);
        page.Controls.Add(_fewestGrid);
        return page;
    }

    private static Label NewSummaryLabel() =>
        new() { AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

    private static DataGridView NewGrid(Point location, Size size) =>
        new()
        {
            Location = location,
            Size = size,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            MultiSelect = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

    private static Dictionary<string, string> RankingHeaders() =>
        new() { ["Position"] = "#", ["Member"] = "Member", ["Username"] = "Username", ["Visits"] = "Visits" };

    private static void Prepare(DataGridView grid, IReadOnlyDictionary<string, string> headers)
    {
        grid.DataBindingComplete += (_, _) =>
        {
            foreach (KeyValuePair<string, string> header in headers)
            {
                if (grid.Columns.Contains(header.Key))
                {
                    grid.Columns[header.Key]!.HeaderText = header.Value;
                }
            }
        };
    }

    private static void ConfigurePicker(DateTimePicker picker, Point location, DateTime max, DateTime value)
    {
        picker.Format = DateTimePickerFormat.Short;
        picker.Location = location;
        picker.Width = 120;
        picker.MaxDate = max;
        picker.Value = value;
    }

    private void Calculate()
    {
        try
        {
            var start = DateOnly.FromDateTime(_startPicker.Value);
            var end = DateOnly.FromDateTime(_endPicker.Value);

            EvolutionResult evolution = _controller.GetEvolution(start, end);
            RankingResult rankings = _controller.GetRankings(start, end);

            _evolutionLabel.Text = evolution.Summary;
            _evolutionGrid.DataSource = evolution.Rows.ToList();

            _rankingLabel.Text = rankings.MySummary;
            _mostGrid.DataSource = rankings.MostVisits.ToList();
            _fewestGrid.DataSource = rankings.FewestVisits.ToList();
        }
        catch (ValidationException ex)
        {
            MessageBox.Show(ex.Message, "Statistics", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (DataAccessException ex)
        {
            MessageBox.Show(ex.Message, "Data error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
