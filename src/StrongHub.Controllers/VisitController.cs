using System.Globalization;
using StrongHub.Data;
using StrongHub.Models;

namespace StrongHub.Controllers;

/// <summary>One row of the visit history shown in the view.</summary>
public record VisitRow(int Id, string Date, string ClassName);

/// <summary>Outcome of registering a visit.</summary>
public record VisitResult(Visit Visit, int VisitsThisWeek, bool RestRecommended);

/// <summary>Data sent to subscribers when a member reaches the weekly visit limit.</summary>
public class WeeklyRestEventArgs : EventArgs
{
    public WeeklyRestEventArgs(int userId, string memberName, DateOnly weekStart, int visits)
    {
        UserId = userId;
        MemberName = memberName;
        WeekStart = weekStart;
        Visits = visits;
    }

    public int UserId { get; }
    public string MemberName { get; }
    public DateOnly WeekStart { get; }
    public int Visits { get; }
}

/// <summary>Registers visits of the logged-in member and counts them per week.</summary>
public class VisitController
{
    public const int RestThreshold = 5;

    private readonly IVisitRepository _visits;
    private readonly IRepository<GymClass> _classes;
    private readonly SessionContext _session;
    private readonly Func<DateOnly> _today;

    public VisitController(
        IVisitRepository visits,
        IRepository<GymClass> classes,
        SessionContext session,
        Func<DateOnly>? today = null)
    {
        _visits = visits;
        _classes = classes;
        _session = session;
        _today = today ?? (() => DateOnly.FromDateTime(DateTime.Today));
    }

    /// <summary>Raised when a member reaches 5 visits in the same week (the timeline will listen to it).</summary>
    public event EventHandler<WeeklyRestEventArgs>? RestRecommended;

    public bool CanRegister => _session.CurrentUser is BasicUser;

    public DateOnly Today => _today();

    public IReadOnlyList<string> GetClassNames() =>
        _classes.GetAll().OrderBy(gymClass => gymClass.Id).Select(gymClass => gymClass.Name).ToList();

    /// <summary>Validates and saves a visit. Throws ValidationException if it breaks a rule.</summary>
    public VisitResult Register(string className, DateOnly date)
    {
        BasicUser member = _session.CurrentUser as BasicUser
                           ?? throw new ValidationException("Only members can register visits.");

        GymClass gymClass = FindClass(className);

        if (date > Today)
        {
            throw new ValidationException("The visit date cannot be in the future.");
        }

        bool duplicated = _visits.GetByUser(member.Id)
            .Any(visit => visit.Date == date && SameText(visit.ClassName, gymClass.Name));
        if (duplicated)
        {
            throw new ValidationException($"You already registered {gymClass.Name} on {date:yyyy-MM-dd}.");
        }

        var visit = new Visit { UserId = member.Id, ClassName = gymClass.Name, Date = date };
        _visits.Add(visit);

        int visitsInWeek = CountVisitsInWeekOf(member.Id, date);
        bool restRecommended = visitsInWeek == RestThreshold;

        if (restRecommended)
        {
            RestRecommended?.Invoke(this,
                new WeeklyRestEventArgs(member.Id, member.Name, WeekHelper.StartOfWeek(date), visitsInWeek));
        }

        return new VisitResult(visit, visitsInWeek, restRecommended);
    }

    /// <summary>Visit history of the logged-in member, newest first. Trainers get an empty list.</summary>
    public IReadOnlyList<VisitRow> GetHistoryRows()
    {
        if (_session.CurrentUser is not BasicUser member)
        {
            return Array.Empty<VisitRow>();
        }

        return _visits.GetByUser(member.Id)
            .Select(visit => new VisitRow(
                visit.Id,
                visit.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                visit.ClassName))
            .ToList();
    }

    public string GetWeekSummary()
    {
        if (_session.CurrentUser is not BasicUser member)
        {
            return "Visit tracking is available for members.";
        }

        int count = CountVisitsInWeekOf(member.Id, Today);
        return $"Visits this week: {count} (rest day recommended at {RestThreshold})";
    }

    private int CountVisitsInWeekOf(int userId, DateOnly date)
    {
        DateOnly weekStart = WeekHelper.StartOfWeek(date);
        DateOnly weekEnd = weekStart.AddDays(6);

        return _visits.GetByUser(userId).Count(visit => visit.Date >= weekStart && visit.Date <= weekEnd);
    }

    private GymClass FindClass(string className) =>
        _classes.GetAll().FirstOrDefault(gymClass => SameText(gymClass.Name, className.Trim()))
        ?? throw new ValidationException("Select a valid gym class.");

    private static bool SameText(string first, string second) =>
        string.Equals(first, second, StringComparison.OrdinalIgnoreCase);
}
