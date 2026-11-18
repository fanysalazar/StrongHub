using StrongHub.Controllers;

namespace StrongHub.Views;

/// <summary>Tabbed window with the history of the logged-in member.</summary>
public class MyHistoryForm : Form
{
    private readonly AppControllers _app;

    public MyHistoryForm(AppControllers app)
    {
        _app = app;
        BuildLayout();
    }

    private void BuildLayout()
    {
        Text = "StrongHub - My history";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        ClientSize = new Size(760, 500);

        var tabs = new TabControl { Location = new Point(15, 15), Size = new Size(730, 425) };
        tabs.TabPages.Add(BuildVisitsTab());
        tabs.TabPages.Add(BuildLatestRoutinesTab());
        tabs.TabPages.Add(BuildMeasurementsTab());
        tabs.TabPages.Add(BuildIncreasesTab());
        Controls.Add(tabs);

        var closeButton = new Button { Text = "Close", Location = new Point(655, 450), Size = new Size(90, 36) };
        closeButton.DialogResult = DialogResult.Cancel;
        CancelButton = closeButton;
        Controls.Add(closeButton);
    }

    private TabPage BuildVisitsTab() => BuildTab(
        "Visit history",
        _app.Visits.GetWeekSummary(),
        _app.Visits.GetHistoryRows().ToList(),
        new Dictionary<string, string> { ["Date"] = "Date", ["ClassName"] = "Class" },
        "Id");

    private TabPage BuildLatestRoutinesTab() => BuildTab(
        "Latest 5 routines",
        _app.RoutineHistory.GetSummary(),
        _app.RoutineHistory.GetLatestRows().ToList(),
        new Dictionary<string, string>
        {
            ["Name"] = "Routine", ["Trainer"] = "Trainer", ["Start"] = "Start",
            ["End"] = "End", ["Status"] = "Status", ["Exercises"] = "Exercises"
        });

    private TabPage BuildMeasurementsTab() => BuildTab(
        "Measurements",
        _app.Measurements.GetSummary(),
        _app.Measurements.GetRows().ToList(),
        new Dictionary<string, string>
        {
            ["Date"] = "Date", ["WeightKg"] = "Weight (kg)",
            ["BodyFatPercent"] = "Body fat (%)", ["MuscleKg"] = "Muscle (kg)"
        });

    private TabPage BuildIncreasesTab() => BuildTab(
        "Load increases",
        _app.RoutineChanges.GetSummary(),
        _app.RoutineChanges.GetRows().ToList(),
        new Dictionary<string, string>
        {
            ["Date"] = "Date", ["Routine"] = "Routine", ["Exercise"] = "Exercise",
            ["PreviousKg"] = "Previous (kg)", ["NewKg"] = "New (kg)", ["Change"] = "Change (kg)"
        });

    private static TabPage BuildTab(
        string title,
        string summary,
        object rows,
        IReadOnlyDictionary<string, string> headers,
        params string[] hiddenColumns)
    {
        var page = new TabPage(title);

        page.Controls.Add(new Label
        {
            Text = summary,
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Location = new Point(10, 12)
        });

        var grid = new DataGridView
        {
            Location = new Point(10, 45),
            Size = new Size(700, 340),
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            MultiSelect = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        grid.DataBindingComplete += (_, _) => ApplyColumnSettings(grid, headers, hiddenColumns);
        page.Controls.Add(grid);
        grid.DataSource = rows;

        return page;
    }

    private static void ApplyColumnSettings(
        DataGridView grid,
        IReadOnlyDictionary<string, string> headers,
        string[] hiddenColumns)
    {
        foreach (KeyValuePair<string, string> header in headers)
        {
            if (grid.Columns.Contains(header.Key))
            {
                grid.Columns[header.Key]!.HeaderText = header.Value;
            }
        }

        foreach (string column in hiddenColumns)
        {
            if (grid.Columns.Contains(column))
            {
                grid.Columns[column]!.Visible = false;
            }
        }

        if (grid.Columns.Contains("Exercises"))
        {
            grid.Columns["Exercises"]!.FillWeight = 260;
        }
    }
}
