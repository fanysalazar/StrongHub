using StrongHub.Data;
using StrongHub.Models;

namespace StrongHub.Controllers;

/// <summary>One row of the shame badge table shown in the view.</summary>
public record ShameRow(string Member, string Username, string Badge, int Visits, string Message);

/// <summary>Rows for a period and how many badges were created by the last evaluation.</summary>
public record ShameResult(IReadOnlyList<ShameRow> Rows, int NewlyAssigned);

/// <summary>Data sent to subscribers when a shame badge is assigned.</summary>
public class ShameBadgeEventArgs : EventArgs
{
    public ShameBadgeEventArgs(int userId, string memberName, UserBadge badge)
    {
        UserId = userId;
        MemberName = memberName;
        Badge = badge;
    }

    public int UserId { get; }
    public string MemberName { get; }
    public UserBadge Badge { get; }
}

/// <summary>Assigns shame badges for a chosen period. Only trainers can run it.</summary>
public class ShameBadgeController
{
    public const int LowestVisitorsCount = 10;

    private readonly IRepository<BasicUser> _users;
    private readonly IRepository<UserBadge> _badges;
    private readonly IVisitRepository _visits;
    private readonly BadgeEvaluator _evaluator;
    private readonly BadgeContextFactory _contextFactory;
    private readonly SessionContext _session;
    private readonly Func<DateOnly> _today;

    public ShameBadgeController(
        IRepository<BasicUser> users,
        IRepository<UserBadge> badges,
        IVisitRepository visits,
        BadgeEvaluator evaluator,
        BadgeContextFactory contextFactory,
        SessionContext session,
        Func<DateOnly>? today = null)
    {
        _users = users;
        _badges = badges;
        _visits = visits;
        _evaluator = evaluator;
        _contextFactory = contextFactory;
        _session = session;
        _today = today ?? (() => DateOnly.FromDateTime(DateTime.Today));
    }

    /// <summary>Raised for every new shame badge (the timeline will listen to it).</summary>
    public event EventHandler<ShameBadgeEventArgs>? ShameBadgeAssigned;

    public bool CanEvaluate => _session.CurrentUser is Trainer;

    public DateOnly Today => _today();

    /// <summary>Evaluates the shame rules for the period and saves the badges that do not exist yet.</summary>
    public ShameResult Evaluate(DateOnly start, DateOnly end)
    {
        if (_session.CurrentUser is not Trainer)
        {
            throw new ValidationException("Only trainers can assign shame badges.");
        }

        var range = new DateRange(start, end);
        IReadOnlySet<int> lowestVisitors = GetLowestVisitorIds(range);
        int newlyAssigned = 0;

        foreach (BasicUser member in _users.GetAll())
        {
            BadgeContext context = _contextFactory.Create(member, range.End, range, lowestVisitors);

            foreach (IBadgeRule rule in _evaluator.GetSatisfiedRules(context, BadgeKind.Shame))
            {
                if (AlreadyAssigned(member.Id, rule.Code, range))
                {
                    continue;
                }

                var badge = new UserBadge
                {
                    UserId = member.Id,
                    BadgeCode = rule.Code,
                    BadgeName = rule.Name,
                    Kind = BadgeKind.Shame,
                    Message = rule.Message,
                    AwardedOn = _today(),
                    RangeStart = range.Start,
                    RangeEnd = range.End
                };

                _badges.Add(badge);
                newlyAssigned++;
                ShameBadgeAssigned?.Invoke(this, new ShameBadgeEventArgs(member.Id, member.Name, badge));
            }
        }

        return new ShameResult(GetRows(range), newlyAssigned);
    }

    /// <summary>Shame badges already assigned for exactly this period.</summary>
    public IReadOnlyList<ShameRow> GetAssignedRows(DateOnly start, DateOnly end) =>
        GetRows(new DateRange(start, end));

    private IReadOnlySet<int> GetLowestVisitorIds(DateRange range) =>
        _users.GetAll()
            .Select(member => (Id: member.Id, Visits: CountVisits(member.Id, range)))
            .OrderBy(entry => entry.Visits)
            .ThenBy(entry => entry.Id)
            .Take(LowestVisitorsCount)
            .Select(entry => entry.Id)
            .ToHashSet();

    private int CountVisits(int userId, DateRange range) =>
        _visits.GetByUser(userId).Count(visit => range.Contains(visit.Date));

    private bool AlreadyAssigned(int userId, string code, DateRange range) =>
        _badges.GetAll().Any(badge =>
            badge.UserId == userId
            && badge.Kind == BadgeKind.Shame
            && badge.BadgeCode == code
            && badge.RangeStart == range.Start
            && badge.RangeEnd == range.End);

    private IReadOnlyList<ShameRow> GetRows(DateRange range)
    {
        Dictionary<int, BasicUser> members = _users.GetAll().ToDictionary(member => member.Id);

        return _badges.GetAll()
            .Where(badge => badge.Kind == BadgeKind.Shame
                            && badge.RangeStart == range.Start
                            && badge.RangeEnd == range.End
                            && members.ContainsKey(badge.UserId))
            .Select(badge =>
            {
                BasicUser member = members[badge.UserId];
                return new ShameRow(member.Name, member.Username, badge.BadgeName,
                    CountVisits(member.Id, range), badge.Message);
            })
            .OrderBy(row => row.Badge)
            .ThenBy(row => row.Visits)
            .ThenBy(row => row.Member)
            .ToList();
    }
}
