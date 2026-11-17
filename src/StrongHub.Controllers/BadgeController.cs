using System.Globalization;
using StrongHub.Data;
using StrongHub.Models;

namespace StrongHub.Controllers;

/// <summary>One row of the badge list shown in the view.</summary>
public record BadgeRow(string Name, string Type, string Date, string Message);

/// <summary>Data sent to subscribers when a member earns an achievement badge.</summary>
public class BadgeEarnedEventArgs : EventArgs
{
    public BadgeEarnedEventArgs(int userId, string memberName, UserBadge badge)
    {
        UserId = userId;
        MemberName = memberName;
        Badge = badge;
    }

    public int UserId { get; }
    public string MemberName { get; }
    public UserBadge Badge { get; }
}

/// <summary>Awards achievement badges to the logged-in member and lists the badges they have.</summary>
public class BadgeController
{
    private readonly IRepository<UserBadge> _badges;
    private readonly BadgeEvaluator _evaluator;
    private readonly BadgeContextFactory _contextFactory;
    private readonly SessionContext _session;
    private readonly Func<DateOnly> _today;

    public BadgeController(
        IRepository<UserBadge> badges,
        BadgeEvaluator evaluator,
        BadgeContextFactory contextFactory,
        SessionContext session,
        Func<DateOnly>? today = null)
    {
        _badges = badges;
        _evaluator = evaluator;
        _contextFactory = contextFactory;
        _session = session;
        _today = today ?? (() => DateOnly.FromDateTime(DateTime.Today));
    }

    /// <summary>Raised for every new achievement badge (the timeline will listen to it).</summary>
    public event EventHandler<BadgeEarnedEventArgs>? BadgeEarned;

    /// <summary>Checks every achievement rule and saves the badges that were not earned before.</summary>
    public IReadOnlyList<UserBadge> EvaluateAchievements()
    {
        BasicUser member = _session.CurrentUser as BasicUser
                           ?? throw new ValidationException("Only members can earn badges.");

        HashSet<string> alreadyEarned = _badges.GetAll()
            .Where(badge => badge.UserId == member.Id && badge.Kind == BadgeKind.Achievement)
            .Select(badge => badge.BadgeCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        DateOnly today = _today();
        BadgeContext context = _contextFactory.Create(member, today);
        var awarded = new List<UserBadge>();

        foreach (IBadgeRule rule in _evaluator.GetSatisfiedRules(context, BadgeKind.Achievement))
        {
            if (alreadyEarned.Contains(rule.Code))
            {
                continue;
            }

            var badge = new UserBadge
            {
                UserId = member.Id,
                BadgeCode = rule.Code,
                BadgeName = rule.Name,
                Kind = BadgeKind.Achievement,
                Message = rule.Message,
                AwardedOn = today
            };

            _badges.Add(badge);
            awarded.Add(badge);
            BadgeEarned?.Invoke(this, new BadgeEarnedEventArgs(member.Id, member.Name, badge));
        }

        return awarded;
    }

    /// <summary>All badges of the logged-in member (achievement and shame), newest first.</summary>
    public IReadOnlyList<BadgeRow> GetMyBadges()
    {
        if (_session.CurrentUser is not BasicUser member)
        {
            return Array.Empty<BadgeRow>();
        }

        return _badges.GetAll()
            .Where(badge => badge.UserId == member.Id)
            .OrderByDescending(badge => badge.AwardedOn)
            .ThenByDescending(badge => badge.Id)
            .Select(badge => new BadgeRow(
                badge.BadgeName,
                badge.Kind == BadgeKind.Achievement ? "Achievement" : "Shame",
                badge.AwardedOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                badge.Message))
            .ToList();
    }

    public string GetSummary()
    {
        if (_session.CurrentUser is not BasicUser member)
        {
            return "Badges are available for members.";
        }

        List<UserBadge> mine = _badges.GetAll().Where(badge => badge.UserId == member.Id).ToList();
        int total = _evaluator.GetRules(BadgeKind.Achievement).Count;
        int earned = mine.Count(badge => badge.Kind == BadgeKind.Achievement);
        int shame = mine.Count(badge => badge.Kind == BadgeKind.Shame);

        return $"Achievement badges: {earned} of {total} earned. Shame badges received: {shame}.";
    }
}
