namespace StrongHub.Controllers;

/// <summary>Week rules shared by visits, badges and statistics (a week goes from Monday to Sunday).</summary>
public static class WeekHelper
{
    public static DateOnly StartOfWeek(DateOnly date)
    {
        int daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-daysSinceMonday);
    }
}
